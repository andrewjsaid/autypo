namespace Autypo;

/// <summary>
/// Represents a set of weights and penalties used to compute a document-query similarity score.
/// </summary>
internal sealed class ScoreProfile
{
    /// <summary>
    /// Weight applied to the user-provided document's base score.
    /// </summary>
    public float DocumentScoreWeight { get; set; } = 24f;

    /// <summary>
    /// Weight for the ratio of matched query tokens to total query tokens.
    /// </summary>
    public float QueryCoverageWeight { get; set; } = 12f;

    /// <summary>
    /// Weight for the ratio of query tokens sequentially matching their predecessor to total query tokens.
    /// </summary>
    public float QuerySequentialWeight { get; set; } = 3f;

    /// <summary>
    /// Weight for the ratio of query tokens matching after their predecessor to total query tokens.
    /// </summary>
    public float QueryInOrderWeight { get; set; } = 3f;

    /// <summary>
    /// Weight for how well the query's token length matches the document's.
    /// <para />
    /// Score is lower if query is too short.
    /// </summary>
    public float QueryLengthFitWeight { get; set; } = 12f;

    /// <summary>
    /// Weight to prefer query tokens matching earlier on
    /// <para/>
    /// Scores higher if first matching query token is earlier.
    /// </summary>
    public float QueryEarlyMatchWeight { get; set; } = 1f;

    /// <summary>
    /// Weight to prefer queries where the last token is not a prefix match.
    /// </summary>
    public float LastTokenPrefixLengthWeight { get; set; } = 1f;

    /// <summary>
    /// Weight for the ratio of matched document tokens to total document tokens.
    /// </summary>
    public float DocumentCoverageWeight { get; set; } = 3f;

    /// <summary>
    /// Weight applied for the binary condition that the document has the query tokens in sequence.
    /// </summary>
    public float DocumentSequentialWeight { get; set; } = 3f;

    /// <summary>
    /// Weight applied for the binary condition that the document has the query tokens in order.
    /// </summary>
    public float DocumentInOrderWeight { get; set; } = 3f;

    /// <summary>
    /// Weight for how well the document's token length matches the query's.
    /// <para />
    /// Score is lower if document is too short.
    /// </summary>
    public float DocumentLengthFitWeight { get; set; } = 1f;

    /// <summary>
    /// Weight for how early on in the document the first match is.
    /// <para/>
    /// Scores higher if first matching document token is earlier.
    /// </summary>
    public float DocumentEarlyMatchWeight { get; set; } = 1f;

    /// <summary>
    /// Penalty multiplier applied to near matches (e.g., synonyms, weak string match).
    /// </summary>
    public float NearPenalty { get; set; } = 0.4f;

    /// <summary>
    /// Penalty multiplier applied to fuzzy matches (e.g., edit distance, misspellings).
    /// </summary>
    public float FuzzyPenalty { get; set; } = 0.1f;

    /// <summary>
    /// Larger offset reduces outsized impact of few tokens.
    /// Specifically transforms e.g. 1/2 into 3/4 (add 2 to numerator and denominator)
    /// </summary>
    public uint LengthFitOffset { get; set; } = 2;
}
