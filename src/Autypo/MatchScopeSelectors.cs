namespace Autypo;

/// <summary>
/// Provides predefined strategies for determining the match scope of query tokens.
/// </summary>
public static class MatchScopeSelectors
{
    /// <summary>
    /// Applies <see cref="MatchScope.Prefix"/> only to the final token in the query;
    /// all other tokens must match fully.
    /// </summary>
    public static MatchScopeSelector PrefixFinalTokenOnly { get; } =
        static (token, context) => token.Contains(context.QueryTokenizedLength - 1) ? MatchScope.Prefix : MatchScope.Full;

    /// <summary>
    /// Applies <see cref="MatchScope.Full"/> to all tokens in the query.
    /// </summary>
    public static MatchScopeSelector Full { get; } =
        static (_, _) => MatchScope.Full;
}
