using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Delegate that determines the match scope to apply for a given query token during evaluation.
/// </summary>
/// <param name="token">The token from the transformed query.</param>
/// <param name="queryContext">The current query context, including token metadata and indexing state.</param>
/// <returns>The <see cref="MatchScope"/> to apply when matching the token.</returns>
public delegate MatchScope MatchScopeSelector(AutypoToken token, AutypoQueryContext queryContext);
