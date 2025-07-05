namespace Autypo.Tokenization;

/// <summary>
/// A specialized tokenizer that delegates the transformation stage entirely to a second "outer" tokenizer,
/// while preserving the structural segmentation of an "inner" tokenizer pipeline.
/// </summary>
/// <remarks>
/// This class allows composition of tokenizers where the inner tokenizer defines segmentation boundaries
/// (e.g., for trivia or position tracking), and the outer tokenizer is responsible for transforming the core
/// token text within each segment.
/// 
/// The <see cref="PipingAutypoTokenizer"/> base class ensures that leading and trailing trivia are preserved,
/// and that tokenized segments from the inner tokenizer are properly scoped for outer processing.
/// </remarks>
/// <param name="inner">The tokenizer responsible for defining the input segmentation.</param>
/// <param name="outer">The tokenizer used to transform the inner segments during <see cref="TokenizeCore"/>.</param>
internal sealed class DecoratingPipingAutypoTokenizer(
    IAutypoTokenizer inner,
    IAutypoTokenizer outer)
    : PipingAutypoTokenizer(inner)
{
    /// <summary>
    /// For each segment emitted by the inner tokenizer, invokes the outer tokenizer on its core token content.
    /// </summary>
    /// <param name="text">The token content of the segment, as emitted by the inner tokenizer.</param>
    protected override void TokenizeCore(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
    {
        outer.Tokenize(text, consumer);
    }
}