namespace Autypo.Tokenization;

/// <summary>
/// Provides a composition pattern for layering tokenizers by piping the output of an inner tokenizer
/// through an outer transformation stage. This base class handles trivia preservation and sequence structure,
/// allowing derived classes to focus purely on transforming token content.
/// </summary>
/// <remarks>
/// The <see cref="PipingAutypoTokenizer"/> ensures that leading and trailing trivia are preserved,
/// and that the segmentation boundaries from the inner tokenizer are respected during outer transformation.
/// Derived types implement <see cref="TokenizeCore"/> to modify the core token contents.
/// </remarks>
/// <param name="inner">The tokenizer whose segments will be used as input to the outer transformation.</param>
internal abstract class PipingAutypoTokenizer(IAutypoTokenizer inner) : IAutypoTokenizer
{
    private readonly TokenSegmentConsumer _innerConsumer = new();

    /// <summary>
    /// Tokenizes the input by first delegating to the inner tokenizer, then piping each emitted segment
    /// through the outer transformation defined by <see cref="TokenizeCore"/>.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    /// <param name="consumer">The consumer that receives token segments from the outer layer.</param>
    public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
    {
        // Reuse internal consumer buffer to collect inner token segments
        _innerConsumer.Reset();

        inner.Tokenize(text, _innerConsumer);

        var outerText = text;
        foreach (var tokenSegment in _innerConsumer.Segments)
        {
            // Append the leading trivia of the outer token segment
            consumer.AcceptTrivia(tokenSegment.LeadingTrivia);

            tokenSegment.Slice(ref outerText, out var innerText);
            
            var started = consumer.CurrentLength;
            
            TokenizeCore(innerText, consumer);

            // Mark any remaining characters in innerText as trivia if not consumed
            consumer.TrivializeRemaining(started, innerText.Length);

            // Also append the trailing trivia of the outer token segment
            consumer.AcceptTrivia(tokenSegment.TrailingTrivia);
        }
    }

    /// <summary>
    /// Performs outer tokenization logic over the text extracted from a single segment produced by the inner tokenizer.
    /// </summary>
    /// <param name="text">The inner token text to be processed further.</param>
    /// <param name="consumer">The consumer that receives the final token segments for this portion.</param>
    protected abstract void TokenizeCore(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer);

}