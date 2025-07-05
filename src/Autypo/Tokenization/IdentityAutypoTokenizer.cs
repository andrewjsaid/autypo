namespace Autypo.Tokenization;

/// <summary>
/// A tokenizer that emits the entire input as a single token without performing segmentation.
/// </summary>
/// <remarks>
/// Useful in scenarios where the entire string should be treated as one unit, such as ID-based lookup,
/// or in downstream processing where custom tokenization occurs later in the pipeline.
/// </remarks>
public sealed class IdentityAutypoTokenizer : IAutypoTokenizer
{
    /// <summary>
    /// A shared singleton instance.
    /// </summary>
    public static IdentityAutypoTokenizer Instance { get; } = new();

    /// <inheritdoc />
    public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
    {
        consumer.Accept(new AutypoTokenSegment(0, text.Length, 0));
    }
}
