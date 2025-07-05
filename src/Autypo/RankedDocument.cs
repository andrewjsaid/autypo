namespace Autypo;

/// <summary>
/// Represents a document that matched a query, along with its score and optional tags.
/// </summary>
/// <remarks>
/// Instances of this type are returned by match rankers to represent the best-scoring match
/// for a given document. A single <see cref="DocumentIndex"/> will appear at most once in the
/// ranked results, with the highest observed score and associated metadata.
/// </remarks>
internal readonly struct RankedDocument(int documentIndex, float score, AutypoTags tags)
{
    /// <summary>
    /// Gets the index of the matched document within the underlying index.
    /// </summary>
    public int DocumentIndex { get; } = documentIndex;

    /// <summary>
    /// Gets the score assigned to this document for the current query.
    /// Higher scores indicate better matches.
    /// </summary>
    public float Score { get; } = score;

    /// <summary>
    /// Gets the tags associated with the match. These may be used to convey
    /// scoring diagnostics or domain-specific metadata.
    /// </summary>
    public AutypoTags Tags { get; } = tags;
}
