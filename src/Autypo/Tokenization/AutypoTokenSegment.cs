namespace Autypo.Tokenization;

/// <summary>
/// Represents a contiguous segment of input text identified during tokenization,
/// including any leading or trailing trivia (e.g., whitespace, punctuation).
/// </summary>
/// <remarks>
/// A token segment divides the input stream into well-formed chunks consisting of:
/// <list type="bullet">
/// <item><description><b>Leading trivia</b> — characters before the token (e.g., whitespace)</description></item>
/// <item><description><b>Token</b> — the actual content to be analyzed or matched</description></item>
/// <item><description><b>Trailing trivia</b> — characters immediately following the token</description></item>
/// </list>
/// 
/// The tokenizer is responsible for emitting a sequence of <see cref="AutypoTokenSegment"/> values
/// such that the sum of their lengths completely spans the input text.
/// </remarks>
public readonly struct AutypoTokenSegment
{
    /// <summary>
    /// Returns an empty token segment containing no token or trivia.
    /// </summary>
    public static AutypoTokenSegment Empty => new(0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="AutypoTokenSegment"/> struct
    /// with the specified trivia and token lengths.
    /// </summary>
    /// <param name="leadingTrivia">The number of characters preceding the token.</param>
    /// <param name="tokenizedLength">The number of characters comprising the token.</param>
    /// <param name="trailingTrivia">The number of characters following the token.</param>
    public AutypoTokenSegment(
        int leadingTrivia,
        int tokenizedLength,
        int trailingTrivia)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(leadingTrivia);
        ArgumentOutOfRangeException.ThrowIfNegative(tokenizedLength);
        ArgumentOutOfRangeException.ThrowIfNegative(trailingTrivia);

        LeadingTrivia = leadingTrivia;
        TokenizedLength = tokenizedLength;
        TrailingTrivia = trailingTrivia;
    }

    /// <summary>
    /// Gets the number of characters of leading trivia (e.g., whitespace or punctuation) before the token.
    /// </summary>
    public int LeadingTrivia { get; }

    /// <summary>
    /// Gets the number of characters representing the token itself.
    /// </summary>
    public int TokenizedLength { get; }

    /// <summary>
    /// Gets the number of characters of trailing trivia (e.g., punctuation or whitespace) following the token.
    /// </summary>
    public int TrailingTrivia { get; }

    /// <summary>
    /// Gets the total number of characters this segment spans, including leading and trailing trivia.
    /// </summary>
    public int TotalLength => LeadingTrivia + TokenizedLength + TrailingTrivia;

    /// <summary>
    /// Gets a value indicating whether this segment includes an actual token.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if <see cref="TokenizedLength"/> is greater than zero; otherwise, <c>false</c>.
    /// Segments with zero-length tokens typically represent trivia-only spans.
    /// </remarks>
    public bool IsToken => TokenizedLength > 0;

    /// <summary>
    /// Extracts the token portion of this segment from the provided input span,
    /// and advances the span past the full segment.
    /// </summary>
    /// <param name="text">
    /// The input text to slice. The reference will be updated to point to the remaining text after this segment.
    /// </param>
    /// <param name="tokenizedText">
    /// Outputs the slice of <paramref name="text"/> corresponding to the token content.
    /// </param>
    public void Slice(
        ref ReadOnlyMemory<char> text,
        out ReadOnlyMemory<char> tokenizedText)
    {
        tokenizedText = text.Slice(LeadingTrivia, TokenizedLength);
        text = text[TotalLength..];
    }
}
