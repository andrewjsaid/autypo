using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Delegate that selects fuzzy matching configuration for an individual token at query time.
/// </summary>
/// <param name="token">The query token being evaluated.</param>
/// <param name="queryContext">The context of the current query, including metadata and token stream.</param>
/// <returns>A <see cref="Fuzziness"/> configuration to apply to the token.</returns>
public delegate Fuzziness FuzzinessSelector(AutypoToken token, AutypoQueryContext queryContext);
