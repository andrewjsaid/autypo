namespace Autypo;

/// <summary>
/// Encapsulates the configuration and token-level details used when processing a query.
/// </summary>
/// <remarks>
/// This includes the matching policy, token ordering strategy, and per-token metadata 
/// derived from analysis, which may inform downstream ranking or diagnostics.
/// </remarks>
internal sealed class QuerySearchInfo
{
    /// <summary>
    /// Gets or sets the policy determining how partial matches are allowed during query evaluation.
    /// </summary>
    public required PartialMatchPolicy PartialMatchPolicy { get; init; }

    /// <summary>
    /// Gets or sets the ordering constraint for how tokens must appear in the document relative to the query.
    /// </summary>
    public required TokenOrdering TokenOrdering { get; init; }

    /// <summary>
    /// Gets or sets token information of tokens extracted pre-transformation.
    /// </summary>
    public required QuerySearchTokenInfo[] ExtractedTokenInfo { get; init; }

    /// <summary>
    /// Gets or sets token information of tokens created after transformation.
    /// </summary>
    public required QuerySearchTokenInfo[] TransformedTokenInfo { get; init; }
}

/// <summary>
/// Contains metadata about a single token during a query search, including scope, fuzziness, and statistics.
/// </summary>
/// <remarks>
/// These details are used to guide matching logic and assist in result ranking and diagnostics.
/// </remarks>
internal struct QuerySearchTokenInfo
{

    /// <summary>
    /// The match scope (full or prefix) applied to this token.
    /// </summary>
    public MatchScope MatchScope;

    /// <summary>
    /// The fuzziness configuration used for token comparison.
    /// </summary>
    public Fuzziness Fuzziness;

    /// <summary>
    /// Accumulated statistics about token distribution and frequency across documents.
    /// </summary>
    public InternalTokenStats Stats;
}
