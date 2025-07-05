using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Represents information about a single token from the indexed document, including match and position metadata.
/// </summary>
public struct DocumentTokenInfo
{
    /// <summary>
    /// The original token in the document, or <c>null</c> if the token was unavailable.
    /// </summary>
    public AutypoToken? Token { get; init; }

    /// <summary>
    /// Indicates whether this token was skipped during transformation (e.g., stop word).
    /// </summary>
    public bool IsSkipped { get; init; }

    /// <summary>
    /// Indicates whether the token exactly matched a query token (edit distance 0).
    /// </summary>
    public bool IsExactMatch { get; init; }

    /// <summary>
    /// Indicates whether the token matched a query token with minimal fuzziness (e.g., edit distance 1).
    /// </summary>
    public bool IsNearMatch { get; init; }

    /// <summary>
    /// Indicates whether the token matched a query token with higher fuzziness (e.g., edit distance ≥ 2).
    /// </summary>
    public bool IsFuzzyMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends a sequence of exact matches.
    /// </summary>
    public bool EndsExactSequentialMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends a sequence of near matches.
    /// </summary>
    public bool EndsNearSequentialMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends a sequence of fuzzy matches.
    /// </summary>
    public bool EndsFuzzySequentialMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends an in-order exact match sequence.
    /// </summary>
    public bool EndsExactInOrderMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends an in-order near match sequence.
    /// </summary>
    public bool EndsNearInOrderMatch { get; init; }

    /// <summary>
    /// Indicates whether this token ends an in-order fuzzy match sequence.
    /// </summary>
    public bool EndsFuzzyInOrderMatch { get; init; }
}
