using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Represents statistics and match information for a query token within a candidate document.
/// </summary>
public struct QueryTokenInfo
{
    /// <summary>
    /// The query token as analyzed and indexed.
    /// </summary>
    public AutypoToken Token { get; init; }

    /// <summary>
    /// Indicates whether any token with the same index matched exactly.
    /// This is reconstructed from overall statistics, not specific to this token instance.
    /// </summary>
    public bool MatchedExact { get; init; }

    /// <summary>
    /// Indicates whether any token with the same index matched with minimal fuzziness.
    /// This is reconstructed from overall statistics, not specific to this token instance.
    /// </summary>
    public bool MatchedNear { get; init; }

    /// <summary>
    /// Indicates whether any token with the same index matched fuzzily.
    /// This is reconstructed from overall statistics, not specific to this token instance.
    /// </summary>
    public bool MatchedFuzzy { get; init; }

    /// <summary>
    /// Indicates whether the token was part of an exact sequential match.
    /// </summary>
    public bool MatchedSequentialExact { get; init; }

    /// <summary>
    /// Indicates whether the token was part of a near sequential match.
    /// </summary>
    public bool MatchedSequentialNear { get; init; }

    /// <summary>
    /// Indicates whether the token was part of a fuzzy sequential match.
    /// </summary>
    public bool MatchedSequentialFuzzy { get; init; }

    /// <summary>
    /// Indicates whether the token was part of an in-order exact match.
    /// </summary>
    public bool MatchedInOrderExact { get; init; }

    /// <summary>
    /// Indicates whether the token was part of an in-order near match.
    /// </summary>
    public bool MatchedInOrderNear { get; init; }

    /// <summary>
    /// Indicates whether the token was part of an in-order fuzzy match.
    /// </summary>
    public bool MatchedInOrderFuzzy { get; init; }

    /// <summary>
    /// The configured match scope used to evaluate this token.
    /// </summary>
    public MatchScope MatchScope { get; init; }

    /// <summary>
    /// The configured fuzziness policy used to evaluate this token.
    /// </summary>
    public Fuzziness Fuzziness { get; init; }

    /// <summary>
    /// Aggregate statistics about this token under exact matching rules.
    /// </summary>
    public QueryTokenDistanceStats ExactStats { get; init; }

    /// <summary>
    /// Aggregate statistics about this token under near matching rules.
    /// </summary>
    public QueryTokenDistanceStats NearStats { get; init; }

    /// <summary>
    /// Aggregate statistics about this token under fuzzy matching rules.
    /// </summary>
    public QueryTokenDistanceStats FuzzyStats { get; init; }
}

/// <summary>
/// Describes the statistical distribution of a token's appearance in the indexed corpus.
/// Used to inform scoring and weighting decisions during ranking.
/// </summary>
public struct QueryTokenDistanceStats
{
    /// <summary>
    /// The number of documents that contain this token.
    /// </summary>
    public int DocumentFrequency { get; init; }

    /// <summary>
    /// The number of document keys that contain this token.
    /// </summary>
    public int DocumentKeyFrequency { get; init; }

    /// <summary>
    /// The total number of times this token appears across all document keys.
    /// </summary>
    public int CollectionFrequency { get; init; }

    /// <summary>
    /// The maximum number of times this token appears in any single document key.
    /// </summary>
    public int MaxTermFrequency { get; init; }

    /// <summary>
    /// The minimum token index (position) in document keys where this token appears.
    /// </summary>
    public int MinPosition { get; init; }

    /// <summary>
    /// The maximum token index (position) in document keys where this token appears.
    /// </summary>
    public int MaxPosition { get; init; }

    /// <summary>
    /// The average of minimum positions where this token appears across document keys.
    /// </summary>
    public float AverageMinPosition { get; init; }

    /// <summary>
    /// The average of maximum positions where this token appears across document keys.
    /// </summary>
    public float AverageMaxPosition { get; init; }

    internal static QueryTokenDistanceStats MapFrom(InternalTokenDistanceStats stats)
    {
        return new QueryTokenDistanceStats
        {
            DocumentFrequency = stats.DocumentFrequency,
            DocumentKeyFrequency = stats.DocumentKeyFrequency,
            CollectionFrequency = stats.CollectionFrequency,
            MaxTermFrequency = stats.MaxTermFrequency,
            MinPosition = stats.MinPosition,
            MaxPosition = stats.MaxPosition,
            AverageMinPosition = stats.AverageMinPosition,
            AverageMaxPosition = stats.AverageMaxPosition
        };
    }
}
