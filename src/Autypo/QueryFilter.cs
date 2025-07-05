namespace Autypo;

/// <summary>
/// A delegate that determines whether a query should be processed.
/// </summary>
/// <param name="query">The input query string.</param>
/// <param name="searchContext">The context of the current search request.</param>
/// <returns>
/// <c>true</c> if the query is considered valid and should be processed; otherwise, <c>false</c>.
/// </returns>
/// <remarks>
/// Useful for filtering out unwanted input such as blank queries, profanity, or queries below a minimum length.
/// </remarks>
public delegate bool QueryFilter(string query, AutypoSearchContext searchContext);
