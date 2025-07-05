namespace Autypo;

/// <summary>
/// Represents a single matched token or token sequence within a specific document key.
/// </summary>
internal struct TokenHit
{
    /// <summary>
    /// Gets or sets the index of the document that contains the match.
    /// </summary>
    public int DocumentIndex;

    /// <summary>
    /// Gets or sets the key index within the document.
    /// <para/>
    /// Documents may expose multiple tokenizable keys. This field ensures that
    /// matches are constrained to a specific key and are not conflated across keys.
    /// </summary>
    public int KeyNum;

    /// <summary>
    /// Gets or sets the length of the matched token sequence.
    /// <para/>
    /// This value reflects how many document tokens were included in the match.
    /// </summary>
    public byte MatchLength;

    /// <summary>
    /// Gets or sets a bitmap representing the start positions of the matched sequence.
    /// <para/>
    /// Only the first bit corresponding to the start of the matched document token span is set.
    /// Used for fast bitmap propagation in ranking or evidence accumulation.
    /// </summary>
    public ulong MatchStartBitmap;

    /// <summary>
    /// Provides equality comparison for <see cref="TokenHit"/> entries based on
    /// document index, key number, and match length.
    /// </summary>
    internal sealed class KeyEqualityComparer : IEqualityComparer<TokenHit>
    {
        /// <summary>
        /// Gets a singleton instance of the comparer.
        /// </summary>
        public static IEqualityComparer<TokenHit> Instance { get; } = new KeyEqualityComparer();

        private KeyEqualityComparer() { }

        /// <inheritdoc/>
        public bool Equals(TokenHit x, TokenHit y)
        {
            return x.DocumentIndex.Equals(y.DocumentIndex)
                   && x.KeyNum.Equals(y.KeyNum)
                   && x.MatchLength.Equals(y.MatchLength);
        }

        /// <inheritdoc/>
        public int GetHashCode(TokenHit obj)
        {
            return HashCode.Combine(obj.DocumentIndex, obj.KeyNum, obj.MatchLength).GetHashCode();
        }
    }
}
