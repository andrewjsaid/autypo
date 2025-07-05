namespace Autypo.Configuration;

/// <summary>
/// Represents the shared configuration metadata for an Autypo index collection.
/// </summary>
/// <remarks>
/// This type is typically used internally or via the generic <see cref="AutypoConfiguration{T}"/>
/// when configuring a collection for a specific data type.
/// </remarks>
internal abstract class AutypoConfiguration
{
    /// <summary>
    /// Gets the unique identifier for this Autypo index collection.
    /// This key is used to distinguish between multiple collections that may index the same data type.
    /// </summary>
    /// <remarks>
    /// The value must be unique across all configured collections and is typically used for routing or diagnostics.
    /// </remarks>
    public required string? CollectionKey { get; init; }

    /// <summary>
    /// Gets the CLR type of the items indexed by this collection.
    /// This is typically set to <c>typeof(T)</c> when using <see cref="AutypoConfiguration{T}"/>.
    /// </summary>
    public required Type CollectionType { get; init; }

    /// <summary>
    /// Gets the strategy used to initialize and hydrate the underlying data source for this collection.
    /// </summary>
    /// <remarks>
    /// This setting controls whether indexing occurs eagerly (at startup), lazily (on first query),
    /// or asynchronously in the background.
    /// </remarks>
    public required InitializationMode InitializationMode { get; init; }

    /// <summary>
    /// Gets the delegate used to determine how many results should be returned for a given query context.
    /// </summary>
    /// <remarks>
    /// Allows fine-grained control over max result counts, potentially varying by query content or caller context.
    /// </remarks>
    public required MaxResultsSelector MaxResultsSelector { get; init; }

    /// <summary>
    /// Gets a value indicating whether tokenized representations of inputs should be retained internally.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, Autypo may preserve intermediate tokenization state for scoring or debugging purposes.
    /// </remarks>
    public required bool KeepTokenization { get; init; }
}

/// <summary>
/// Represents the strongly-typed configuration for an Autypo index collection for items of type <typeparamref name="T"/>.
/// Defines how data is sourced, indexed, and queried for a specific domain model.
/// </summary>
/// <typeparam name="T">The type of data items managed by this collection. Must be non-nullable.</typeparam>
internal sealed class AutypoConfiguration<T> : AutypoConfiguration where T : notnull
{
    /// <summary>
    /// Gets the factory delegate that provides the data source for items to be indexed.
    /// </summary>
    /// <remarks>
    /// The <paramref name="object"/> parameter corresponds to <see cref="DataSourceContext"/>,
    /// and may be used to inject services or contextual dependencies.
    /// </remarks>
    public required Func<object?, IAutypoDataSource<T>> DataSourceFactory { get; init; }

    /// <summary>
    /// Gets the optional context object passed to the <see cref="DataSourceFactory"/> at initialization time.
    /// </summary>
    /// <remarks>
    /// This is typically used to supply external dependencies such as service containers or tenant metadata.
    /// </remarks>
    public required object? DataSourceContext { get; init; }

    /// <summary>
    /// Gets the handler used to manage queries that occur before the data source has been initialized.
    /// </summary>
    /// <remarks>
    /// This allows graceful fallback behavior during startup or failure conditions.
    /// For example, it may return cached results, empty lists, or throw a specific exception.
    /// </remarks>
    public required UninitializedDataSourceHandler<T>? UninitializedDataSourceHandler { get; init; }

    /// <summary>
    /// Gets the list of index configurations that define how data is projected and queried.
    /// Each index represents an independent view or transformation of the data.
    /// </summary>
    public required IReadOnlyList<AutypoIndexConfiguration<T>> Indices { get; init; }

    /// <summary>
    /// Gets the optional predicate used to determine whether an item should be included in the index.
    /// </summary>
    /// <remarks>
    /// If the predicate returns <c>false</c> for a given item, it will be excluded from all configured indices.
    /// </remarks>
    public required Func<T, bool>? ShouldIndex { get; init; }

    /// <summary>
    /// Gets the optional delegate used to compute a relevance score for indexed documents.
    /// </summary>
    public required DocumentScorer<T>? DocumentScorer { get; init; }

    /// <summary>
    /// Gets the strategy used to produce results when the query is empty or null.
    /// </summary>
    /// <remarks>
    /// Common strategies include returning popular results, recent items, or no results at all.
    /// </remarks>
    public required EmptyQueryHandler<T> EmptyQueryHandler { get; init; }
}
