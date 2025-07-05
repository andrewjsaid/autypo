using System.Diagnostics;
using System.Text;

namespace Autypo.Tokenization;

/// <summary>
/// Represents a token produced during text analysis in Autypo.
/// </summary>
/// <remarks>
/// A token captures a segment of text along with positional and structural metadata,
/// including whether it originated from the source document, was synthesized through transformation,
/// or spans multiple original tokens. Tokens are produced by the tokenizer and optionally
/// modified or replaced by transformers.
/// </remarks>
[DebuggerDisplay("{Text}")]
public sealed class AutypoToken
{
    private AutypoTags _tags;

    internal AutypoToken(
        int sequenceStart,
        int sequenceLength,
        ReadOnlyMemory<char> text,
        AutypoTags tags)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sequenceStart);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequenceLength);

        SequenceStart = sequenceStart;
        SequenceLength = sequenceLength;
        Text = text;
        _tags = tags;
    }

    /// <summary>
    /// Gets the starting position of this token in the tokenized input stream.
    /// </summary>
    public int SequenceStart { get; }

    /// <summary>
    /// Gets the number of contiguous sequence positions this token spans.
    /// A single-token span has a length of 1.
    /// </summary>
    public int SequenceLength { get; }

    /// <summary>
    /// Gets the final sequence index covered by this token.
    /// Equal to <see cref="SequenceStart"/> + <see cref="SequenceLength"/> - 1.
    /// </summary>
    public int SequenceEnd => SequenceStart + SequenceLength - 1;

    /// <summary>
    /// Determines whether the specified sequence number falls within the bounds of this token.
    /// </summary>
    /// <param name="sequenceNumber">The sequence index to check.</param>
    /// <returns><c>true</c> if the sequence number is within this token's span; otherwise, <c>false</c>.</returns>
    public bool Contains(int sequenceNumber) => unchecked((uint)(sequenceNumber - SequenceStart) < (uint)SequenceLength);

    /// <summary>
    /// Indicates whether this token was produced directly by tokenization,
    /// before any transformation.
    /// </summary>
    public bool IsOriginal { get; internal set; }

    /// <summary>
    /// Indicates whether this original token was removed during transformation.
    /// Only relevant when <see cref="IsOriginal"/> is <c>true</c>.
    /// </summary>
    public bool IsDeleted { get; internal set; }

    /// <summary>
    /// Indicates whether this token spans multiple original sequence positions.
    /// </summary>
    public bool IsCompoundToken => SequenceLength > 1;

    /// <summary>
    /// Gets a bitmask representing the starting position of this token in the input sequence.
    /// </summary>
    /// <remarks>
    /// The bit at the <see cref="SequenceStart"/> index is set to <c>1</c>.
    /// This is useful for tracking token boundaries during analysis or transformation,
    /// especially when evaluating overlaps or gaps between tokens.
    /// </remarks>
    internal ulong SequenceStartBitmap => unchecked(1ul << SequenceStart);

    /// <summary>
    /// Gets a bitmask representing the full range of sequence positions covered by this token.
    /// </summary>
    /// <remarks>
    /// All bits corresponding to the token’s sequence span are set to <c>1</c>.
    /// This is used for efficient bitmap-based token coverage and skipping logic
    /// during match evaluation and result composition.
    /// </remarks>
    internal ulong SequenceBitmap => unchecked(((1ul << SequenceLength) - 1) << (int)SequenceStart);

    /// <summary>
    /// Gets the raw text content of this token.
    /// </summary>
    public ReadOnlyMemory<char> Text { get; }

    /// <summary>
    /// Gets a mutable reference to the tags associated with this token.
    /// </summary>
    public ref AutypoTags Tags => ref _tags;

    /// <summary>
    /// Returns a copy of this token with the specified new text, preserving sequence and tags.
    /// </summary>
    /// <param name="text">The replacement text for the new token.</param>
    /// <returns>A new token instance with updated text.</returns>
    public AutypoToken WithText(ReadOnlyMemory<char> text)
    {
        return new(SequenceStart, SequenceLength, text, Tags.Clone());
    }

    /// <summary>
    /// Creates a new compound token that spans the sequence range of the given tokens.
    /// Optionally replaces the token text and merges tags.
    /// </summary>
    /// <param name="source">The input tokens to concatenate.</param>
    /// <param name="text">Optional replacement text. If not provided, tokens are concatenated with spaces.</param>
    /// <param name="copyTags">Whether to copy tags from the input tokens to the resulting token.</param>
    /// <returns>A new compound token spanning the full range of the input tokens.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is empty.</exception>
    public static AutypoToken Concat(IEnumerable<AutypoToken> source, ReadOnlyMemory<char>? text = null, bool copyTags = false)
    {
        var min = int.MaxValue;
        var max = 0;

        var tags = new AutypoTags();
        var sb = text is null ? new StringBuilder() : null;

        foreach (var token in source)
        {
            if (sb is not null)
            {
                sb.Append(token.Text);
                sb.Append(' ');
            }

            min = Math.Min(min, token.SequenceStart);
            max = Math.Max(max, token.SequenceEnd);

            if (copyTags)
            {
                tags.CopyFrom(token.Tags);
            }
        }

        if (text is null)
        {
            Debug.Assert(sb is not null);
            // Remove trailing space from final joined token
            sb.Length--;
            text = sb.ToString().AsMemory();
        }

        if (min is int.MaxValue)
        {
            throw new ArgumentException(Resources.AutypoToken_SourceIsEmpty, nameof(source));
        }

        Debug.Assert(min <= max);

        return new AutypoToken(min, 1 + max - min, text.Value, tags);
    }
}
