namespace Autypo;

/// <summary>
/// Represents a document and its associated score in a search or suggestion result set.
/// </summary>
/// <typeparam name="T">The type of the indexed document.</typeparam>
/// <remarks>
/// This struct is typically used in fallback scenarios such as empty-query handlers,
/// or in advanced search pipelines where precomputed scores are available or required.
/// Consumers may use the score to influence result ranking or presentation.
/// </remarks>
public readonly struct ScoredDocument<T>
{
    /// <summary>
    /// Gets the original document that was indexed.
    /// </summary>
    public required T Document { get; init; }

    /// <summary>
    /// Gets the static or precomputed score associated with this document.
    /// </summary>
    /// <remarks>
    /// This value may originate from the indexing phase (e.g., popularity, recency)
    /// or be attached dynamically for fallback and filtering use cases.
    /// Higher values typically indicate stronger relevance.
    /// </remarks>
    public required float Score { get; init; }
}
