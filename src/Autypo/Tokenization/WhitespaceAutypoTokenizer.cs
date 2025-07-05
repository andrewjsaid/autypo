using System.Buffers;

namespace Autypo.Tokenization;

/// <summary>
/// A tokenizer that splits input text by Unicode-defined whitespace characters.
/// </summary>
/// <remarks>
/// Recognizes a wide range of whitespace characters beyond ASCII space,
/// including non-breaking and multilingual separators.
/// Preserves trivia segments (whitespace) between tokens.
/// </remarks>
public sealed class WhitespaceAutypoTokenizer : IAutypoTokenizer
{
    private static readonly SearchValues<char> _whitespace = SearchValues.Create(
            ['\u0009', '\u000A', '\u000B', '\u000C', '\u000D',
             '\u0020', '\u0085', '\u00A0', '\u1680', '\u2000',
             '\u2001', '\u2002', '\u2003', '\u2004', '\u2005',
             '\u2006', '\u2007', '\u2008', '\u2009', '\u200A',
             '\u2028', '\u2029', '\u202F', '\u205F', '\u3000']);

    /// <summary>
    /// A shared singleton instance.
    /// </summary>
    public static WhitespaceAutypoTokenizer Instance { get; } = new();

    /// <inheritdoc />
    public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
    {
        while (text.Length > 0)
        {
            var span = text.Span;

            var leadingTriviaLength = span.IndexOfAnyExcept(_whitespace);
            if (leadingTriviaLength == -1)
            {
                return;
            }

            var tokenizedLength = span[leadingTriviaLength..].IndexOfAny(_whitespace);
            if (tokenizedLength == -1)
            {
                // only tokenized text remains
                consumer.Accept(new AutypoTokenSegment(
                    leadingTriviaLength,
                    span.Length - leadingTriviaLength,
                    trailingTrivia: 0));

                break;
            }

            var tokenSegment = new AutypoTokenSegment(
                leadingTriviaLength,
                tokenizedLength,
                trailingTrivia: 0);
            
            consumer.Accept(tokenSegment);

            tokenSegment.Slice(ref text, out _);
        }
    }
}
