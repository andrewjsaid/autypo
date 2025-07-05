using System.Diagnostics;
using Autypo.Configuration;
using Levenshtypo;

namespace Autypo;

/// <summary>
/// Represents the abstract base class for search engines built on top of Autypo.
/// </summary>
internal abstract class AutypoSearch
{
    /// <summary>
    /// The internal activity source used for tracing and diagnostics.
    /// </summary>
    protected internal static readonly ActivitySource _activitySource = new("Autypo");

    private readonly AutypoConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutypoSearch"/> class using the provided configuration.
    /// </summary>
    /// <param name="configuration">The root configuration object for this search instance.</param>
    internal AutypoSearch(AutypoConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the configured initialization mode for the underlying Autypo instance.
    /// </summary>
    internal InitializationMode InitializationMode => _configuration.InitializationMode;

    /// <summary>
    /// Requests that the index be reloaded, optionally scheduling another reload if one is already in progress.
    /// </summary>
    /// <param name="triggerAgainIfAlreadyRunning">If <c>true</c>, another reload is scheduled after the current finishes.</param>
    /// <param name="cancellationToken">A cancellation token for aborting the operation.</param>
    internal abstract Task TriggerReloadAsync(bool triggerAgainIfAlreadyRunning, CancellationToken cancellationToken);

    /// <summary>
    /// Forces the index to reload from its data source.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for aborting the operation.</param>
    internal abstract Task ReloadAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Provides search capabilities over a collection of documents of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The document type being indexed and searched. Must be non-nullable.</typeparam>
internal sealed class AutypoSearch<T> : AutypoSearch, IAutypoSearch<T> where T : notnull
{
    private readonly AutypoConfiguration<T> _configuration;
    private readonly AsyncReloadGate _reloadGate = new();
    // This is wrapped in a class so that it can be set atomically
    private Data? _data;

    internal AutypoSearch(AutypoConfiguration<T> configuration) : base(configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public IEnumerable<AutypoSearchResult<T>> Search(string query, AutypoSearchContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        context ??= new();

        using var activity = _activitySource.StartActivity("Autypo.Search");
        activity?.SetTag("autypo.query", query);

        if (string.Equals(query, string.Empty))
        {
            return HandleEmptyQuery(context);
        }

        var data = _data;

        if (data is null)
        {
            activity?.SetTag("autypo.uninitialized", true);
            return HandleUninitialized(_configuration, query, context);
        }

        activity?.SetTag("autypo.documents", data.DocumentMetadata.Length);

        IMatchRanker ranker = _configuration.MaxResultsSelector(context) switch
        {
            null => new UnboundedMatchRanker(),
            1 => new SingleMatchRanker(),
            { } maxResults => new BoundedMatchRanker(maxResults)
        };

        var hitCount = 0;

        var prioritizer = IndexPrioritizer<T>.Create(data.IndexReaders, context);

        var candidateMatch = new MatchCandidate<T>();

        while (prioritizer.GetNext(ranker.Count > 0, out var readerIndex))
        {
            var indexConfiguration = _configuration.Indices[readerIndex];
            var candidateTagger = indexConfiguration.CandidateTagger;
            var candidateFilter = indexConfiguration.CandidateFilter;
            var candidateScorer = indexConfiguration.CandidateScorer;

            var indexReader = data.IndexReaders[readerIndex];

            var queriedIndices = indexReader.Search(query, context);

            foreach (var queriedIndex in queriedIndices)
            {
                foreach (var match in queriedIndex.Matches)
                {
                    hitCount++;

                    candidateMatch.SetMatch(
                        data.DocumentMetadata,
                        queriedIndex.IndexDocumentMetadata,
                        queriedIndex.IndexKeyDocumentMetadata,
                        match,
                        queriedIndex.QueryContext,
                        queriedIndex.QuerySearchInfo);

                    if (candidateTagger is not null)
                    {
                        candidateTagger(candidateMatch, queriedIndex.QueryContext);
                    }

                    if (candidateFilter is null || candidateFilter(candidateMatch, queriedIndex.QueryContext))
                    {
                        var score = candidateScorer(candidateMatch, queriedIndex.QueryContext);

                        ranker.Process(match.DocumentIndex, score, candidateMatch.Tags);
                    }
                }
            }
        }

        var results = new List<AutypoSearchResult<T>>();

        var ranked = ranker.GetRankedDocuments();

        foreach (var rankedDocument in ranked)
        {
            var value = data.DocumentMetadata[rankedDocument.DocumentIndex].Document;

            var result = new AutypoSearchResult<T>(value, rankedDocument.Tags);
            results.Add(result);
        }

        if (activity is not null)
        {
            activity.SetTag("autypo.results", results.Count);
            activity.SetTag("autypo.hits", hitCount);
        }

        return results;
    }

    /// <inheritdoc />
    public async ValueTask<IEnumerable<AutypoSearchResult<T>>> SearchAsync(string term, AutypoSearchContext? context = null, CancellationToken cancellationToken = default)
    {
        if (_data is null)
        {
            await TriggerReloadAsync(triggerAgainIfAlreadyRunning: false, cancellationToken);
        }

        return Search(term, context ?? new());
    }

    private IEnumerable<AutypoSearchResult<T>> HandleEmptyQuery(AutypoSearchContext context)
    {
        var queryContext = new AutypoQueryContext(
            rawQuery: string.Empty,
            query: string.Empty,
            indexName: null,
            extractedQueryTokens: [],
            transformedQueryTokens: [],
            queryTokenizedLength: 0,
            metadata: context.Metadata,
            documentMetadata: _data!.DocumentMetadata,
            indexedDocumentCount: 0,
            indexedDocumentKeysCount: 0
        );

        var results = new List<AutypoSearchResult<T>>();

        foreach (var document in _configuration.EmptyQueryHandler(queryContext))
        {
            results.Add(new AutypoSearchResult<T>(document, tags: AutypoTags.None));
        }

        return results;
    }

    private static IEnumerable<AutypoSearchResult<T>> HandleUninitialized(AutypoConfiguration<T> configuration, string query, AutypoSearchContext context)
    {
        var uninitializedStateHandler = configuration.UninitializedDataSourceHandler;
        Debug.Assert(uninitializedStateHandler is not null, "Configuration constraints mean that both _levenshtrie and _uninitializedStateHandler should never be null");
        if (uninitializedStateHandler is null)
        {
            throw new InvalidOperationException(Resources.Search_Uninitialized);
        }

        return uninitializedStateHandler(query, context);
    }

    /// <inheritdoc />
    internal override async Task TriggerReloadAsync(bool triggerAgainIfAlreadyRunning, CancellationToken cancellationToken)
    {
        await _reloadGate.TriggerAsync(this, triggerAgainIfAlreadyRunning, cancellationToken);
    }

    /// <inheritdoc />
    internal override async Task ReloadAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("Autypo.Reload");

        var documents = await _configuration.DataSourceFactory(_configuration.DataSourceContext).LoadDocumentsAsync(cancellationToken);

        var data = IndexData(documents, cancellationToken);
        _data = data;

        activity?.SetTag("autypo.documents", data.DocumentMetadata.Length);
    }

    private Data IndexData(IEnumerable<T> documents, CancellationToken cancellationToken)
    {
        var documentMetadata = BuildDocumentMetadata(documents);

        var keyMetadata = new List<IndexKeyDocumentMetadata>(documentMetadata.Length);

        var indexReaders = new IndexReader<T>[_configuration.Indices.Count];
        var i = 0;

        foreach (var indexConfiguration in _configuration.Indices)
        {
            var trie = Levenshtrie.CreateEmptySet<TokenHit>(ignoreCase: !indexConfiguration.EnableCaseSensitivity, resultComparer: TokenHit.KeyEqualityComparer.Instance);
            var indexMetadata = new IndexDocumentMetadata[documentMetadata.Length];
            keyMetadata.Clear();

            var indexedDocumentCount = 0;
            var indexedDocumentKeysCount = 0;

            var indexWriter = new IndexWriter<T>(indexConfiguration, trie, keepTokenization: _configuration.KeepTokenization);
            for (var documentIndex = 0; documentIndex < documentMetadata.Length; documentIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyMetadataStartIndex = keyMetadata.Count;
                indexWriter.Index(documentMetadata[documentIndex].Document, documentIndex, keyMetadata);

                var keyMetadataLength = keyMetadata.Count - keyMetadataStartIndex;

                if (keyMetadataLength > 0)
                {
                    indexedDocumentCount++;
                    indexedDocumentKeysCount += keyMetadataLength;
                }

                indexMetadata[documentIndex] = new IndexDocumentMetadata(keyMetadataStartIndex, keyMetadataLength);
            }

            indexReaders[i++] = new IndexReader<T>(
                indexConfiguration,
                trie,
                documentMetadata,
                indexMetadata,
                keyMetadata.ToArray(),
                indexedDocumentCount,
                indexedDocumentKeysCount);
        }

        return new Data(indexReaders, documentMetadata);
    }

    private DocumentMetadata<T>[] BuildDocumentMetadata(IEnumerable<T> documents)
    {
        var results = new List<DocumentMetadata<T>>();

        foreach (var document in documents)
        {
            if (_configuration.ShouldIndex is { } shouldIndex && !shouldIndex(document))
            {
                continue;
            }

            var score = _configuration.DocumentScorer is { } scorer ? scorer(document) : DocumentScorers.DefaultScore;
            results.Add(new DocumentMetadata<T>(document, score));
        }

        return results.ToArray();
    }

    private class Data(IndexReader<T>[] indexReaders, DocumentMetadata<T>[] documentMetadata)
    {
        public IndexReader<T>[] IndexReaders { get; } = indexReaders;

        public DocumentMetadata<T>[] DocumentMetadata { get; } = documentMetadata;
    }
}
