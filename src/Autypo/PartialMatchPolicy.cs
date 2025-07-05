namespace Autypo;

/// <summary>
/// Specifies which query tokens must be matched for a document to be considered a result.
/// Supports required tokens, percentage-based thresholds, or both.
/// </summary>
/// <remarks>
/// Partial matching allows flexibility in search behavior when not all query tokens must be satisfied.
/// There are three common strategies:
/// <list type="bullet">
/// <item><description>All tokens must match: <see cref="AllQueryTokensRequired"/></description></item>
/// <item><description>At least one token must match: <see cref="SomeQueryTokensRequired(float)"/> with threshold 0</description></item>
/// <item><description>Specific tokens must match, others contribute to a percentage threshold</description></item>
/// </list>
/// This struct is evaluated per-query. Required token indexes refer to their position after tokenization.
/// </remarks>
public readonly struct PartialMatchPolicy
{
    /// <summary>
    /// Requires every query token to be matched.
    /// This is equivalent to setting a <see cref="Threshold"/> of 1.0.
    /// </summary>
    public static PartialMatchPolicy AllQueryTokensRequired() => new(1f, 0ul);


    /// <summary>
    /// Allows partial matches if a certain percentage of query tokens are matched.
    /// No specific tokens are marked as required.
    /// </summary>
    /// <param name="threshold">
    /// The fraction of query tokens (between 0 and 1) that must match.
    /// A value of 0 allows matches with just one token.
    /// A value of 1 behaves like <see cref="AllQueryTokensRequired"/>.
    /// </param>
    public static PartialMatchPolicy SomeQueryTokensRequired(float threshold = 0f) => new(threshold, 0ul);

    internal PartialMatchPolicy(float threshold, ulong requiredTokenMask)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1);

        Threshold = threshold;
        RequiredTokenMask = requiredTokenMask;
    }

    /// <summary>
    /// The fraction of query tokens that must be matched.
    /// Ignored for tokens explicitly marked as required.
    /// </summary>
    public float Threshold { get; }

    internal ulong RequiredTokenMask { get; }

    /// <summary>
    /// Marks a specific token as required, by its zero-based index in the tokenized query.
    /// </summary>
    /// <param name="requiredTokenIndex">The position of the token in the original query.</param>
    /// <returns>A new <see cref="PartialMatchPolicy"/> with the additional required token.</returns>
    public PartialMatchPolicy WithRequiredQueryToken(int requiredTokenIndex) 
        => new(Threshold, RequiredTokenMask | (1ul << requiredTokenIndex));
}
