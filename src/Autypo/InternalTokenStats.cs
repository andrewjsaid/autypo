namespace Autypo;

/// <summary>
/// Aggregated statistics for a token at different fuzzy match distances.
/// </summary>
internal struct InternalTokenStats
{
    /// <summary>
    /// Statistics collected from exact (distance = 0) matches.
    /// </summary>
    public InternalTokenDistanceStats ExactStats;

    /// <summary>
    /// Statistics collected from near (distance ≤ 1) matches.
    /// </summary>
    public InternalTokenDistanceStats NearStats;

    /// <summary>
    /// Statistics collected from all (fuzzy) matches.
    /// </summary>
    public InternalTokenDistanceStats FuzzyStats;
}

/// <summary>
/// Describes token frequency and positional statistics at a specific edit distance.
/// </summary>
internal struct InternalTokenDistanceStats
{
    /// <summary>
    /// Number of documents containing at least one match.
    /// </summary>
    public int DocumentFrequency;

    /// <summary>
    /// Number of document keys containing at least one match.
    /// </summary>
    public int DocumentKeyFrequency;
    
    /// <summary>
    /// Total number of matches across all keys in the collection.
    /// </summary>
    public int CollectionFrequency;

    /// <summary>
    /// Highest number of matches in any single document key.
    /// </summary>
    public int MaxTermFrequency;

    /// <summary>
    /// Lowest position (token index) where a match was found.
    /// </summary>
    public int MinPosition;

    /// <summary>
    /// Highest position (token index) where a match was found.
    /// </summary>
    public int MaxPosition;

    /// <summary>
    /// Average minimum token index where a match was found across all keys.
    /// </summary>
    public float AverageMinPosition;

    /// <summary>
    /// Average maximum token index where a match was found across all keys.
    /// </summary>
    public float AverageMaxPosition;
}
