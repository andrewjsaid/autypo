namespace Autypo.Tokenization;

/// <summary>
/// Applies tokenization and transformation to input text, producing a stream of output tokens.
/// </summary>
/// <remarks>
/// <see cref="AutypoTextAnalyzer"/> coordinates the <see cref="IAutypoTokenizer"/> and
/// <see cref="IAutypoTokenTransformer"/> components to segment input into discrete tokens
/// and apply token-level transformations (such as normalization, filtering, or synonym expansion).
/// Transformed tokens are returned in evaluation-ready form via <see cref="Analyze"/>.
/// </remarks>
internal sealed class AutypoTextAnalyzer(IAutypoTokenizer tokenizer, IAutypoTokenTransformer transformer)
{
    /// <summary>
    /// Creates a default analyzer that tokenizes by whitespace and performs no transformation.
    /// </summary>
    /// <returns>
    /// A factory delegate that produces a new <see cref="AutypoTextAnalyzer"/> instance
    /// using a <see cref="WhitespaceAutypoTokenizer"/> and <see cref="IdentityAutypoTokenTransformer"/>.
    /// </returns>
    internal static Func<AutypoTextAnalyzer> CreateDefaultTextAnalyzerFactory()
         => static () => new(new WhitespaceAutypoTokenizer(), IdentityAutypoTokenTransformer.Instance);

    /// <summary>
    /// Analyzes the given input text by applying tokenization and transformation pipelines.
    /// </summary>
    /// <param name="text">The text to tokenize and transform.</param>
    /// <returns>
    /// An <see cref="AutypoTextAnalyzerResult"/> containing the original and transformed tokens.
    /// Returned tokens are sorted first by sequence number, then by length, with no missing sequences.
    /// </returns>
    public AutypoTextAnalyzerResult Analyze(ReadOnlyMemory<char> text)
    {
        var (extracted, transformed) = Process(text);

        return AutypoTextAnalyzerResult.Create(extracted, transformed);
    }

    private (AutypoToken[] extracted, AutypoToken[] transformed) Process(ReadOnlyMemory<char> text)
    {
        var tokenSegmentConsumer = new TokenSegmentConsumer();
        var tokenConsumer = new TokenConsumer();

        var extractedTokens = new List<AutypoToken>();
        var transformedTokens = new List<AutypoToken>();

        var startLength = tokenSegmentConsumer.CurrentLength;

        tokenizer.Tokenize(text, tokenSegmentConsumer);
        
        // Untokenized text is implicitly trivia
        tokenSegmentConsumer.TrivializeRemaining(startLength, text.Length);

        var hasChanges = false;

        var nextText = text;
        int sequence = 0;

        foreach (var tokenSegment in tokenSegmentConsumer.Segments)
        {
            tokenSegment.Slice(ref nextText, out var tokenizedText);

            if (!tokenSegment.IsToken)
            {
                continue;
            }

            var token = new AutypoToken(
                sequenceStart: sequence++,
                sequenceLength: 1,
                tokenizedText,
                tags: AutypoTags.None);

            token.IsOriginal = true;

            extractedTokens.Add(token);

            tokenConsumer.Reset();

            transformer.Transform(token, tokenConsumer);

            var consumedTokens = tokenConsumer.Tokens;
            if (consumedTokens.Count is not 1 || consumedTokens[0] != token)
            {
                hasChanges = true;

                if (!consumedTokens.Contains(token))
                {
                    token.IsDeleted = true;
                }
            }

            transformedTokens.AddRange(consumedTokens);
        }

#if DEBUG
        // Sanity check: extracted tokens must be in contiguous sequence order
        for (var i = 0; i < extractedTokens.Count; i++)
        {
            System.Diagnostics.Debug.Assert(extractedTokens[i].SequenceStart == i);
        }
#endif

        var extracted = extractedTokens.ToArray();

        // Avoid copying if no transformation occurred
        var transformed = hasChanges ? transformedTokens.ToArray() : extracted;

        return (extracted, transformed);
    }
}