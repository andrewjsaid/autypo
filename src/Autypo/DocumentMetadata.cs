namespace Autypo;

/// <summary>
/// Represents metadata for a document across all indices.
/// </summary>
/// <typeparam name="T">The type of the indexed document.</typeparam>
/// <remarks>
/// This struct is used internally to retain the canonical form of a document
/// and its associated score, independent of index-specific representations.
/// It supports global filtering, scoring, and fallback result mechanisms.
/// </remarks>
internal readonly struct DocumentMetadata<T>(T document, float score)
{
    /// <summary>
    /// Gets the original document that was indexed.
    /// </summary>
    public T Document { get; } = document;

    /// <summary>
    /// Gets the precomputed score assigned to this document.
    /// </summary>
    /// <remarks>
    /// This score is used during query evaluation and result projection,
    /// particularly in empty-query fallback scenarios or global document-level prioritization.
    /// </remarks>
    public float Score { get; } = score;
}
