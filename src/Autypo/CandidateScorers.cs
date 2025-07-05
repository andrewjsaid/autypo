namespace Autypo;

/// <summary>
/// Provides default scoring logic for candidate document matches.
/// </summary>
internal static class CandidateScorers
{
    private static readonly ScoreProfile _defaultScoreProfile = new();

    /// <summary>
    /// Computes a relevance score for the given <see cref="MatchCandidate{T}"/> using the default <see cref="ScoreProfile"/>.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="candidate">The candidate document to score.</param>
    /// <param name="queryContext">The query context in which the match occurred.</param>
    /// <returns>A floating-point score representing the relevance of the match.</returns>
    public static float DefaultScorer<T>(MatchCandidate<T> candidate, AutypoQueryContext queryContext)
    {
        return DefaultScorer(candidate, queryContext, _defaultScoreProfile);
    }

    /// <summary>
    /// Computes a relevance score for the given <see cref="MatchCandidate{T}"/> using the specified <see cref="ScoreProfile"/>.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="candidate">The candidate document to score.</param>
    /// <param name="queryContext">The query context in which the match occurred.</param>
    /// <param name="profile">The score profile defining the weight of different match factors.</param>
    /// <returns>A floating-point score representing the relevance of the match.</returns>
    /// <remarks>
    /// This method uses a weighted linear model based on various match characteristics:
    /// <list type="bullet">
    /// <item><description>Document score (base likelihood)</description></item>
    /// <item><description>Query and document coverage (number of matched tokens)</description></item>
    /// <item><description>Sequential and in-order match alignment</description></item>
    /// <item><description>Length fit between query and document</description></item>
    /// <item><description>Match position (favoring earlier matches)</description></item>
    /// <item><description>Suffix quality of the final token (for autocomplete)</description></item>
    /// </list>
    /// Each component is scaled and combined using the weights and penalties defined in the <paramref name="profile"/>.
    /// </remarks>
    public static float DefaultScorer<T>(MatchCandidate<T> candidate, AutypoQueryContext queryContext, ScoreProfile profile)
    {
        // For now just a linear score until we design a better scoring algorithm

        var score = 0f;

        score += profile.DocumentScoreWeight * candidate.DocumentScore;

        score += profile.QueryCoverageWeight *
                    (candidate.QueryExactMatchCount
                    + (candidate.QueryNearMatchCount * profile.NearPenalty)
                    + (candidate.QueryFuzzyMatchCount * profile.FuzzyPenalty)
                    ) / candidate.QueryTokenizedLength;

        score += profile.QuerySequentialWeight *
                    (candidate.QueryExactSequentialMatchCount
                    + (candidate.QueryNearMatchCount * profile.NearPenalty)
                    + (candidate.QueryFuzzySequentialMatchCount * profile.FuzzyPenalty)
                    ) / candidate.QueryTokenizedLength;

        score += profile.QueryInOrderWeight *
                    (candidate.QueryExactInOrderMatchCount
                    + (candidate.QueryNearInOrderMatchCount * profile.NearPenalty)
                    + (candidate.QueryFuzzyInOrderMatchCount * profile.FuzzyPenalty)
                    ) / candidate.QueryTokenizedLength;

        score += profile.QueryLengthFitWeight * Math.Min(1, (profile.LengthFitOffset + candidate.QueryTokenizedLength) / (float)(profile.LengthFitOffset + candidate.DocumentTokenizedLength));

        score += profile.QueryEarlyMatchWeight * (1 - (candidate switch
        {
            { QueryExactMatchCount: > 0 } => candidate.QueryFirstExactMatchIndex,
            { QueryNearMatchCount: > 0 } => candidate.QueryFirstNearMatchIndex,
            _ => candidate.QueryFirstFuzzyMatchIndex,
        }) / (float)candidate.QueryTokenizedLength);

        score += profile.LastTokenPrefixLengthWeight / (1f + candidate.GetExtractedQueryTokenInfo(candidate.QueryTokenizedLength - 1) switch
        {
            { MatchedExact: true } => candidate.QueryFinalExactTokenBestSuffixLength,
            { MatchedNear: true } => candidate.QueryFinalNearTokenBestSuffixLength / profile.NearPenalty,
            { MatchedFuzzy: true } => candidate.QueryFinalFuzzyTokenBestSuffixLength / profile.FuzzyPenalty,
            _ => 0,
        });

        score += profile.DocumentCoverageWeight *
                    (candidate.DocumentExactMatchCount
                    + (candidate.DocumentNearMatchCount * profile.NearPenalty)
                    + (candidate.DocumentFuzzyMatchCount * profile.FuzzyPenalty)
                    ) / candidate.DocumentTokenizedLength;

        score += profile.DocumentSequentialWeight * candidate switch
        {
            { DocumentHasExactSequentialMatch: true } => 1,
            { DocumentHasNearSequentialMatch: true } => profile.NearPenalty,
            { DocumentHasFuzzySequentialMatch: true } => profile.FuzzyPenalty,
            _ => 0
        };

        score += profile.DocumentInOrderWeight * candidate switch
        {
            { DocumentHasExactInOrderMatch: true } => 1,
            { DocumentHasNearInOrderMatch: true } => profile.NearPenalty,
            { DocumentHasFuzzyInOrderMatch: true } => profile.FuzzyPenalty,
            _ => 0
        };

        score += profile.DocumentLengthFitWeight * Math.Min(1, (profile.LengthFitOffset + candidate.DocumentTokenizedLength) / (float)(profile.LengthFitOffset + candidate.QueryTokenizedLength));

        score += profile.DocumentEarlyMatchWeight * (1 - (candidate switch
        {
            { DocumentExactMatchCount: > 0 } => candidate.DocumentFirstExactMatchIndex,
            { DocumentNearMatchCount: > 0 } => candidate.DocumentFirstNearMatchIndex,
            _ => candidate.DocumentFirstFuzzyMatchIndex,
        }) / (float)candidate.DocumentTokenizedLength);

        return score;
    }
}
