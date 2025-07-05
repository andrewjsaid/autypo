namespace Autypo;

/// <summary>
/// Represents a method that applies custom tags to a match candidate during the search pipeline.
/// </summary>
/// <typeparam name="T">The type of the candidate document.</typeparam>
/// <param name="candidate">The current match candidate being evaluated.</param>
/// <param name="queryContext">The context of the query in which the match occurred.</param>
/// <remarks>
/// Tagging is commonly used to classify or annotate matches based on business rules,
/// for use in scoring, filtering, or display.
/// </remarks>
public delegate void CandidateTagger<T>(MatchCandidate<T> candidate, AutypoQueryContext queryContext);
