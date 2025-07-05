namespace Autypo;

/// <summary>
/// Represents a method that determines whether a match candidate should be included in final scoring.
/// </summary>
/// <typeparam name="T">The type of the candidate document.</typeparam>
/// <param name="candidate">The match candidate being evaluated.</param>
/// <param name="queryContext">The context of the query in which the match occurred.</param>
/// <returns><c>true</c> to include the candidate; otherwise, <c>false</c>.</returns>
/// <remarks>
/// Filtering occurs after tagging but before scoring. It can be used to exclude
/// unwanted matches based on token alignment, metadata, or external business logic.
/// </remarks>
public delegate bool CandidateFilter<T>(MatchCandidate<T> candidate, AutypoQueryContext queryContext);
