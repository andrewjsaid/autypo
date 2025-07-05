namespace Autypo.Configuration;

/// <summary>
/// Provides convenience methods for configuring token matching, ordering, and partial behavior on an Autypo index.
/// </summary>
public static class AutypoIndexConfigurationBuilderExtensions
{

    #region Fuzziness

    /// <summary>
    /// Disables fuzzy matching. All tokens must match exactly.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Use this for identifiers, codes, or other domains where exact spelling is required.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithNoFuzziness<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithFuzziness(static (q, _) => Fuzziness.None);
    }

    /// <summary>
    /// Enables fuzzy matching with a fixed maximum edit distance.
    /// </summary>
    /// <param name="maxEditDistance">The maximum number of allowed edits (insertions, deletions, substitutions).</param>
    /// <param name="allowTransposition">Whether character swaps are treated as single edits.</param>
    /// <returns>The current builder instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxEditDistance"/> is negative.</exception>
    /// <remarks>
    /// Higher values increase recall but may reduce precision. Typical values range from 0–2 depending on token length and tolerance.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithFuzziness<T>(this AutypoIndexConfigurationBuilder<T> @this, int maxEditDistance, bool allowTransposition = false) where T : notnull
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxEditDistance);
        return @this.WithFuzziness((_, _) => new(maxEditDistance, allowTransposition));
    }

    #endregion

    #region MatchScope

    /// <summary>
    /// Requires the final token in the query to fully match an indexed token.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This disables the default prefix behavior for the final token and is useful when autocomplete is not desired.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithFinalTokenFullMatchScope<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithMatchScope(MatchScopeSelectors.Full);
    }

    /// <summary>
    /// Allows the final token in the query to match the prefix of an indexed token.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This is the default behavior and is commonly used for autocomplete or incremental search scenarios.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithFinalTokenPrefixMatchScope<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithMatchScope(MatchScopeSelectors.PrefixFinalTokenOnly);
    }

    #endregion

    #region Partial Token Matching

    /// <summary>
    /// Enables partial query token matching based on a minimum match percentage.
    /// </summary>
    /// <param name="percent">
    /// The fraction (0–1) of query tokens that must be matched. Defaults to <c>0</c>, which requires at least one match.
    /// </param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This is useful when handling noisy or incomplete queries. To require all tokens, use <c>1.0</c>.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithPartialTokenMatching<T>(this AutypoIndexConfigurationBuilder<T> @this, float percent = 0f) where T : notnull
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(percent, 0f);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 1f);
        return @this.WithPartialTokenMatching((_, _) => PartialMatchPolicy.SomeQueryTokensRequired(threshold: percent));
    }

    #endregion

    #region TokenOrdering

    /// <summary>
    /// Allows query tokens to match in any order in the indexed document.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// All tokens must still match, but their order and spacing is not enforced.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithUnorderedTokenOrdering<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithTokenOrdering(static _ => TokenOrdering.Unordered);
    }

    /// <summary>
    /// Requires query tokens to appear in the exact order and adjacent to one another in the indexed document.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Use this for strict phrase matches or where precise structure matters (e.g., addresses, code).
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithStrictSequenceTokenOrdering<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithTokenOrdering(static _ => TokenOrdering.StrictSequence);
    }

    /// <summary>
    /// Requires query tokens to appear in the same order as the indexed document, but allows intervening tokens.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This is the default token ordering strategy and works well for most autocomplete and search scenarios.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithInOrderTokenOrdering<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.WithTokenOrdering(static _ => TokenOrdering.InOrder);
    }

    #endregion

    #region QueryFilter

    /// <summary>
    /// Adds a query filter that requires a minimum number of characters before a query is eligible for matching.
    /// </summary>
    /// <param name="length">The minimum number of characters required for the query to be considered valid.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This prevents short or trivial queries from triggering expensive indexing or search operations.  
    /// By default, Autypo applies a minimum length of <c>3</c> characters unless this filter is overridden.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> AddMinimumLengthQueryFilter<T>(this AutypoIndexConfigurationBuilder<T> @this, int length) where T : notnull
    {
        return @this.AddQueryFilter(QueryFilters.MinimumLength(length));
    }

    /// <summary>
    /// Removes the default minimum query length restriction, allowing queries of any length to be processed.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// By default, Autypo filters out queries shorter than 3 characters.  
    /// This method disables that behavior entirely, enabling processing of single-character or even empty queries.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> AddNoMinimumLengthQueryFilter<T>(this AutypoIndexConfigurationBuilder<T> @this) where T : notnull
    {
        return @this.AddQueryFilter(static (_, _) => true);
    }

    #endregion

    #region PrioritySelector

    /// <summary>
    /// Sets a fixed search priority for this index.
    /// </summary>
    /// <param name="priority">A numeric value indicating this index’s relative importance. Higher is better.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Indices with higher priority are searched first. If a result is found, lower-priority indices are skipped.
    /// </remarks>
    public static AutypoIndexConfigurationBuilder<T> WithPriority<T>(this AutypoIndexConfigurationBuilder<T> @this, int priority) where T : notnull
    {
        return @this.WithPriority(_ => priority);
    }

    #endregion
}