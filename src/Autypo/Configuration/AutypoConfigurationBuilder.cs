namespace Autypo.Configuration;

/// <summary>
/// Provides a fluent builder API for configuring Autypo.
/// </summary>
/// <typeparam name="T">The type of data item to be indexed. Must be non-nullable.</typeparam>
public sealed class AutypoConfigurationBuilder<T> where T : notnull
{
    private const int DefaultMaxResults = 10;

    private Func<object?, IAutypoDataSource<T>>? _dataSourceFactory;
    private object? _dataSourceContext;
    private InitializationMode _initializationMode = InitializationMode.Eager;
    private UninitializedDataSourceHandler<T>? _uninitializedDataSourceHandler;
    private readonly List<AutypoIndexConfigurationBuilder<T>> _indexBuilders = [];
    private Func<T, bool>? _shouldIndex;
    private DocumentScorer<T>? _documentScorer;
    private MaxResultsSelector? _maxResultsSelector;
    private bool _keepTokenization;
    private EmptyQueryHandler<T>? _emptyQueryHandler;
    private readonly List<AutypoRefreshToken> _refreshTokens = [];

    internal AutypoConfigurationBuilder()
    {

    }

    /// <summary>
    /// Specifies the delegate used to construct the data source for this collection.
    /// </summary>
    /// <param name="dataSourceFactory">
    /// A factory function that produces an <see cref="IAutypoDataSource{T}"/> instance,
    /// optionally using a provided context object.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithDataSourceFactory(Func<object?, IAutypoDataSource<T>> dataSourceFactory)
    {
        _dataSourceFactory = dataSourceFactory;
        return this;
    }

    /// <summary>
    /// Sets the context object that will be passed to the data source factory.
    /// </summary>
    /// <param name="dataSourceContext">
    /// An optional context object for use during data source initialization.
    /// This is commonly used by integrations such as <c>Autypo.AspNetCore</c> to inject a <c>ServiceProvider</c>.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithDataSourceContext(object dataSourceContext)
    {
        _dataSourceContext = dataSourceContext;
        return this;
    }

    /// <summary>
    /// Configures how and when the data source will be initialized, and how to handle uninitialized access.
    /// </summary>
    /// <param name="initializationMode">The strategy for when indexing should occur.</param>
    /// <param name="onUninitialized">The behavior to apply when the index is accessed before initialization completes.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if an incompatible <paramref name="onUninitialized"/> value is used with the selected <paramref name="initializationMode"/>.
    /// </exception>
    public AutypoConfigurationBuilder<T> WithInitializationMode(InitializationMode initializationMode, UninitializedBehavior onUninitialized)
    {
        _initializationMode = initializationMode;
        _uninitializedDataSourceHandler = onUninitialized switch
        {
            UninitializedBehavior.None when (initializationMode is InitializationMode.Eager) => null,
            UninitializedBehavior.None => throw new ArgumentException(Resources.AutypoConfiguration_UninitializationMode_Incompatible, nameof(UninitializedBehavior)),
            UninitializedBehavior.Throw => UninitializedDataSourceHandlers.Throw<T>(),
            UninitializedBehavior.ReturnEmpty => UninitializedDataSourceHandlers.ReturnEmpty<T>(),
            _ => throw new NotSupportedException($"UninitializedBehavior {onUninitialized} is not supported.")
        };
        return this;
    }

    /// <summary>
    /// Sets a predicate to determine whether a given item should be included in the index.
    /// </summary>
    /// <param name="shouldIndex">A function that returns <c>true</c> for items to include; <c>false</c> to exclude.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithShouldIndex(Func<T, bool> shouldIndex)
    {
        _shouldIndex = shouldIndex;
        return this;
    }

    /// <summary>
    /// Specifies a delegate used to compute relevance scores for indexed documents.
    /// </summary>
    /// <param name="documentScorer">
    /// A scoring function that assigns a numeric score to a document. Higher scores indicate higher relevance.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithDocumentScorer(DocumentScorer<T> documentScorer)
    {
        _documentScorer = documentScorer;
        return this;
    }

    /// <summary>
    /// Configures how many results should be returned for each query.
    /// </summary>
    /// <param name="selector">A delegate that determines the maximum result count based on the query context.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithMaxResults(MaxResultsSelector selector)
    {
        _maxResultsSelector = selector;
        return this;
    }

    /// <summary>
    /// Adds an index definition for this collection.
    /// </summary>
    /// <param name="keySelector">
    /// A function that extracts a key from each item. The key does not need to be unique.
    /// </param>
    /// <param name="configureIndex">A callback used to configure the index’s tokenization, fields, and scoring.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithIndex(
        Func<T, string?> keySelector,
        Action<AutypoIndexConfigurationBuilder<T>> configureIndex)
    {
        var indexBuilder = new AutypoIndexConfigurationBuilder<T>(keySelector);
        configureIndex(indexBuilder);
        _indexBuilders.Add(indexBuilder);
        return this;
    }

    /// <summary>
    /// Enables optional metadata enrichment features such as token preservation and match context tracking.
    /// </summary>
    /// <param name="enrichment">
    /// A set of flags that determine which metadata should be collected during indexing and search.
    /// </param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Enabling enrichment may increase memory usage and processing time.
    /// Use this only if your application needs token-level metadata for scoring, diagnostics, or filtering.
    /// </remarks>
    public AutypoConfigurationBuilder<T> WithEnrichedMetadata(AutypoMetadataEnrichment enrichment)
    {
        _keepTokenization = (enrichment | AutypoMetadataEnrichment.IncludeDocumentTokenText) is not 0;
        return this;
    }

    /// <summary>
    /// Configures the behavior to use when a query is empty.
    /// </summary>
    /// <param name="emptyQueryHandler">
    /// A delegate that returns results when the query string is empty.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoConfigurationBuilder<T> WithEmptyQueryHandling(EmptyQueryHandler<T> emptyQueryHandler)
    {
        _emptyQueryHandler = emptyQueryHandler;
        return this;
    }

    /// <summary>
    /// Registers a refresh token that allows external control over when the index should be rebuilt.
    /// </summary>
    /// <param name="token">A token representing a refresh policy or trigger.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This enables reactive updates to the underlying index.
    /// For example:
    /// <code>
    /// var token = new AutypoRefreshToken();
    /// var completer = await AutypoFactory.CreateCompleteAsync(config => config
    ///     .WithDataSource(new MySource())
    ///     .UseRefreshToken(token)
    ///     .WithEmptyQueryHandling(context => context.GetDocuments&lt;MyType&gt;().Select(x => x.Document).ToArray()));
    ///
    /// // ... later
    /// await token.RefreshAsync(); // triggers a reindex
    /// </code>
    /// </remarks>
    public AutypoConfigurationBuilder<T> UseRefreshToken(AutypoRefreshToken token)
    {
        _refreshTokens.Add(token);
        return this;
    }

    /// <summary>
    /// Finalizes the builder and constructs the immutable configuration object.
    /// </summary>
    /// <param name="collectionKey">An optional identifier for this configuration instance.</param>
    /// <returns>A <see cref="BuildResult"/> containing the finalized configuration and associated refresh tokens.</returns>
    /// <exception cref="AutypoConfigurationException">
    /// Thrown if required components such as the data source or index definitions are missing.
    /// </exception>
    internal BuildResult Build(string? collectionKey)
    {
        if (_indexBuilders.Count == 0)
        {
            if (typeof(T) == typeof(string))
            {
                _indexBuilders.Add(new AutypoIndexConfigurationBuilder<T>(static s => (string)(object)s));
            }
            else
            {
                throw new AutypoConfigurationException(
                    string.Format(Resources.AutypoConfiguration_MissingKey, typeof(T).Name));
            }
        }

        var indexes = new List<AutypoIndexConfiguration<T>>();

        foreach (var indexBuilder in _indexBuilders)
        {
            indexes.Add(indexBuilder.Build());
        }

        var config = new AutypoConfiguration<T>
        {
            CollectionKey = collectionKey,
            CollectionType = typeof(T),
            InitializationMode = _initializationMode,
            DataSourceFactory = _dataSourceFactory ?? throw new AutypoConfigurationException(Resources.AutypoConfiguration_MissingDataSource),
            DataSourceContext = _dataSourceContext,
            UninitializedDataSourceHandler = _uninitializedDataSourceHandler,
            Indices = indexes,
            ShouldIndex = _shouldIndex,
            DocumentScorer = _documentScorer,
            MaxResultsSelector = _maxResultsSelector ?? (static _ => DefaultMaxResults),
            KeepTokenization = _keepTokenization,
            EmptyQueryHandler = _emptyQueryHandler ?? (static _ => []),
        };

        return new BuildResult
        {
            Config = config,
            RefreshTokens = [.._refreshTokens]
        };
    }

    /// <summary>
    /// Represents the result of building an <see cref="AutypoConfiguration{T}"/> instance.
    /// </summary>
    internal class BuildResult
    {
        public required AutypoConfiguration<T> Config { get; init; }
        public required AutypoRefreshToken[] RefreshTokens { get; init; }
    }
}
