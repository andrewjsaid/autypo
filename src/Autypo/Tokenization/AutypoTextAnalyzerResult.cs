namespace Autypo.Tokenization;

/// <summary>
/// Represents the result of analyzing text with an <see cref="AutypoTextAnalyzer"/>,
/// including both extracted and transformed tokens.
/// </summary>
/// <remarks>
/// This type is produced by the analysis pipeline and holds token metadata
/// for subsequent indexing or matching steps. It captures both the original
/// token stream as extracted by the tokenizer and the transformed stream
/// after token-level modifications have been applied.
/// </remarks>
internal sealed class AutypoTextAnalyzerResult
{
    private AutypoTextAnalyzerResult(
        AutypoToken[] extractedTokens,
        AutypoToken[] transformedTokens,
        ulong skippedBitmap,
        int tokenizedLength)
    {
        ExtractedTokens = extractedTokens;
        TransformedTokens = transformedTokens;
        SkippedBitmap = skippedBitmap;
        TokenizedLength = tokenizedLength;
    }

    /// <summary>
    /// Gets the original tokens emitted by the tokenizer, before any transformation.
    /// </summary>
    public AutypoToken[] ExtractedTokens { get; }

    /// <summary>
    /// Gets the transformed tokens produced after applying token transformations.
    /// </summary>
    public AutypoToken[] TransformedTokens { get; }

    /// <summary>
    /// Gets a bitmask indicating which token sequence positions were not represented
    /// in the transformed output. Used to detect dropped or elided tokens.
    /// </summary>
    internal ulong SkippedBitmap { get; }

    /// <summary>
    /// Gets the total length of the token sequence after transformation.
    /// This may include gaps if tokens were removed or collapsed.
    /// </summary>
    internal int TokenizedLength { get; }

    /// <summary>
    /// Creates a new <see cref="AutypoTextAnalyzerResult"/> by computing metadata
    /// from the extracted and transformed token streams.
    /// </summary>
    /// <param name="extracted">The original tokens produced by tokenization.</param>
    /// <param name="transformed">The transformed tokens, which may differ from the originals.</param>
    /// <returns>A new <see cref="AutypoTextAnalyzerResult"/> instance describing the full analysis.</returns>
    public static AutypoTextAnalyzerResult Create(AutypoToken[] extracted, AutypoToken[] transformed)
    {
        ulong exists = 0ul;
        int tokenizedLength = 0;

        foreach (var token in transformed)
        {
            exists |= token.SequenceBitmap;

            if (tokenizedLength < token.SequenceEnd + 1)
            {
                tokenizedLength = token.SequenceEnd + 1;
            }
        }

        var skippedBitmap = ((1u << (int)tokenizedLength) - 1u) & ~exists;
        return new(extracted, transformed, skippedBitmap, tokenizedLength);
    }
}