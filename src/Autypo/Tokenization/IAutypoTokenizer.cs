using System.Diagnostics;

namespace Autypo.Tokenization;

/// <summary>
/// Defines a tokenizer that segments input text into logical tokens and associated trivia.
/// </summary>
/// <remarks>
/// Implementations are responsible for analyzing a span of text and reporting each
/// token (and any leading/trailing trivia) via the provided <see cref="TokenSegmentConsumer"/>.
/// </remarks>
public interface IAutypoTokenizer
{
    /// <summary>
    /// Tokenizes the specified text into segments and reports them to the provided consumer.
    /// </summary>
    /// <param name="text">The input text to tokenize.</param>
    /// <param name="consumer">The consumer that accepts token segments.</param>
    void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer);
}

/// <summary>
/// Collects <see cref="AutypoTokenSegment"/> instances during tokenization,
/// tracking both structural tokens and non-token trivia (e.g., whitespace or punctuation).
/// </summary>
public sealed class TokenSegmentConsumer
{
    private readonly List<AutypoTokenSegment> _segments = [];
    private int _acceptedLength;

    /// <summary>
    /// Adds a token segment to the consumer.
    /// </summary>
    /// <param name="tokenSegment">The token segment to accept.</param>
    public void Accept(AutypoTokenSegment tokenSegment)
    {
        _segments.Add(tokenSegment);
        _acceptedLength += tokenSegment.TotalLength;
    }

    /// <summary>
    /// Accepts a trivia-only segment of the given length (i.e., no actual token).
    /// </summary>
    public void AcceptTrivia(int length) => Accept(
        new AutypoTokenSegment(
            leadingTrivia: length,
            tokenizedLength: 0,
            trailingTrivia: 0));

    /// <summary>
    /// The total number of characters covered by all accepted segments so far.
    /// </summary>
    public int CurrentLength => _acceptedLength;

    /// <summary>
    /// Ensures any remaining uncovered span is filled with a trailing trivia segment.
    /// </summary>
    /// <param name="started">The start offset (relative to the full input span).</param>
    /// <param name="length">The expected length of the upcoming segment.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the parameters would exceed or misalign with the accepted character count.
    /// </exception>
    public void TrivializeRemaining(int started, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(started, _acceptedLength);
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);

        var total = started + length;

        if (_acceptedLength < total)
        {
            AcceptTrivia(total - _acceptedLength);
        }

        Debug.Assert(_acceptedLength == total);
    }

    /// <summary>
    /// Clears the current segments and resets internal counters.
    /// </summary>
    public void Reset()
    {
        _segments.Clear();
        _acceptedLength = 0;
    }

    /// <summary>
    /// Gets the list of accepted token segments.
    /// </summary>
    public IReadOnlyList<AutypoTokenSegment> Segments => _segments;
}
