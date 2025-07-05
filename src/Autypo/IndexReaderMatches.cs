namespace Autypo;

/// <summary>
/// Represents the search result matches produced by a single index reader during a query evaluation.
/// </summary>
/// <remarks>
/// This includes the matching evidence, search context, and token-level metadata,
/// and serves as the bridge between the lower-level index and higher-level ranking logic.
/// </remarks>
internal sealed class IndexReaderMatches
{
    /// <summary>
    /// Gets or sets the context of the query used to perform the search.
    /// </summary>
    public required AutypoQueryContext QueryContext { get; init; }

    /// <summary>
    /// Gets or sets the set of token-level match evidence gathered during query processing.
    /// </summary>
    public required IEnumerable<TokenMatchEvidence> Matches { get; init; }

    /// <summary>
    /// Gets or sets the document-level metadata used by this index.
    /// </summary>
    public required IndexDocumentMetadata[] IndexDocumentMetadata { get; init; }

    /// <summary>
    /// Gets or sets the key-level metadata associated with each document key in this index.
    /// </summary>
    public required IndexKeyDocumentMetadata[] IndexKeyDocumentMetadata { get; init; }

    /// <summary>
    /// Gets or sets the search metadata describing the strategy and token-level decisions used for this query.
    /// </summary>
    public required QuerySearchInfo QuerySearchInfo { get; init; }
}
