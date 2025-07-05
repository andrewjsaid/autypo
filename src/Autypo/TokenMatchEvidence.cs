namespace Autypo;

/// <summary>
/// Aggregates match evidence across all edit distances for a specific document and key.
/// </summary>
internal struct TokenMatchEvidence
{
    /// <summary>
    /// The index of the document in the current index segment.
    /// </summary>
    public int DocumentIndex;

    /// <summary>
    /// The key number within the document, used to disambiguate multi-key documents.
    /// </summary>
    public int KeyNum;

    /// <summary>
    /// Evidence for exact (0-edit distance) matches.
    /// </summary>
    public TokenMatchDistanceEvidence Exact;

    /// <summary>
    /// Evidence for near matches (1-edit distance).
    /// </summary>
    public TokenMatchDistanceEvidence Near;

    /// <summary>
    /// Evidence for fuzzy matches (any edit distance).
    /// </summary>
    public TokenMatchDistanceEvidence Fuzzy;
}

/// <summary>
/// Encodes bitwise evidence for token-level match behavior under a specific edit distance.
/// </summary>
internal struct TokenMatchDistanceEvidence
{
    /// <summary>
    /// Bitmap where bit <c>1 &lt;&lt; N</c> represents whether
    /// the Nth query token had any match at this distance.
    /// </summary>
    public ulong QueryMatchBitmap;

    /// <summary>
    /// Bitmap where bit <c>1 &lt;&lt; N</c> represents whether
    /// the final query token matched the document token at index N.
    /// </summary>
    public ulong QueryFinalTokenMatchBitmap;

    /// <summary>
    /// For the final query token, this is the smallest suffix length
    /// required to match a document token. Used in ranking for
    /// prefix matches and partial token matching.
    /// </summary>
    public byte QueryFinalTokenBestSuffixLength;

    /// <summary>
    /// Bitmap where flag 1 &lt;&lt; N represents whether
    /// the query token at index N matched sequentially
    /// from the query token at index N - 1.
    /// <para />
    /// If token at index N - 1 did not match then N will not
    /// be a sequential match either.
    /// <para />
    /// The first token which matches is considered sequential,
    /// hence in nearly all instances there will be at least 1
    /// flag set.
    /// </summary>
    public ulong QuerySequentialMatchBitmap;

    /// <summary>
    /// Bitmap where flag 1 &lt;&lt; N represents whether
    /// the query token at index N matched in order
    /// from the query token at index N - 1.
    /// <para />
    /// If token at index N - 1 did not match then N will not
    /// be an In Order match either.
    /// <para />
    /// The first token which matches is considered In Order,
    /// hence in nearly all instances there will be at least 1
    /// flag set.
    /// </summary>
    public ulong QueryInOrderMatchBitmap;

    /// <summary>
    /// Bitmap where flag 1 &lt;&lt; N represents whether
    /// the token with sequence N was found by the
    /// query.
    /// </summary>
    public ulong DocumentMatchBitmap;

    /// <summary>
    /// Bitmap where flag 1 &lt;&lt; N represents whether
    /// all document tokens up to sequence N was found by
    /// the query.
    /// </summary>
    public ulong DocumentSequentialMatchBitmap;

    /// <summary>
    /// A bitmask where bit <c>1 &lt;&lt; N</c> is set if
    /// the query tokens appear in order, ending with a
    /// match at document token position <c>N</c>.
    /// <para/>
    /// If partial token matching is enabled, the bit for
    /// the final matched query token's position is set.
    /// </summary>
    public ulong DocumentInOrderMatchBitmap;
}
