namespace Autypo;

/// <summary>
/// Represents a method that computes a relevance score for a match candidate.
/// </summary>
/// <typeparam name="T">The type of the candidate document.</typeparam>
/// <param name="candidate">The match candidate to score.</param>
/// <param name="queryContext">The context of the query in which the match occurred.</param>
/// <returns>A floating-point score indicating the strength or relevance of the match.</returns>
/// <remarks>
/// A scorer is invoked after filtering and tagging. Custom scorers can use token-level
/// match information and metadata to prioritize results.
/// </remarks>
public delegate float CandidateScorer<T>(MatchCandidate<T> candidate, AutypoQueryContext queryContext);
