using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Metadata about a document specific to one index.
/// </summary>
internal readonly struct IndexDocumentMetadata(
    int keyMetadataStartIndex,
    int keyMetadataLength)
{
    /// <summary>
    /// Returns a span of key-level metadata associated with this document.
    /// </summary>
    /// <param name="full">The full buffer of <see cref="IndexKeyDocumentMetadata"/>.</param>
    /// <returns>A slice containing only the relevant entries for this document.</returns>
    public ReadOnlySpan<IndexKeyDocumentMetadata> GetMetadata(ReadOnlySpan<IndexKeyDocumentMetadata> full)
    {
        return full.Slice(keyMetadataStartIndex, keyMetadataLength);
    }

    /// <summary>
    /// Returns metadata for a specific key of this document.
    /// </summary>
    /// <param name="full">The full metadata buffer.</param>
    /// <param name="keyNum">The key number to retrieve metadata for.</param>
    /// <returns>The metadata associated with the specified key.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified key is not found.</exception>
    public IndexKeyDocumentMetadata GetMetadata(ReadOnlySpan<IndexKeyDocumentMetadata> full, int keyNum)
    {
        foreach (var keyMetadata in full.Slice(keyMetadataStartIndex, keyMetadataLength))
        {
            if (keyMetadata.KeyNum == keyNum)
            {
                return keyMetadata;
            }
        }

        throw new InvalidOperationException();
    }
}

/// <summary>
/// Metadata about a key used to index a document.
/// </summary>
internal readonly struct IndexKeyDocumentMetadata(
    int keyNum,
    ulong skippedBitmap,
    int tokenizedLength,
    AutypoToken[]? extractedTokens,
    AutypoToken[]? transformedTokens)
{
    /// <summary>
    /// The key number associated with this metadata.
    /// </summary>
    public int KeyNum { get; } = keyNum;

    /// <summary>
    /// The tokens extracted before transformation (e.g., raw tokens).
    /// </summary>
    public AutypoToken[]? ExtractedTokens { get; } = extractedTokens;

    /// <summary>
    /// The tokens after transformation (e.g., lowercased, stemmed).
    /// </summary>
    public AutypoToken[]? TransformedTokens { get; } = transformedTokens;

    /// <summary>
    /// The number of tokens resulting from tokenization.
    /// </summary>
    public int TokenizedLength { get; } = tokenizedLength;

    /// <summary>
    /// A bitmap indicating which token positions were skipped during indexing.
    /// </summary>
    public ulong SkippedBitmap { get; } = skippedBitmap;
}
