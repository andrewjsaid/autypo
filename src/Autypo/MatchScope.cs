namespace Autypo;

/// <summary>
/// Specifies how a query token is matched against indexed tokens.
/// </summary>
public enum MatchScope
{
    /// <summary>
    /// The query token must match the entire indexed token.
    /// Supports exact or fuzzy matches depending on configuration.
    /// </summary>
    Full,

    /// <summary>
    /// The query token may match only the beginning (prefix) of an indexed token.
    /// Commonly used to support incremental search or autocomplete.
    /// </summary>
    Prefix
}
