namespace Autypo;

/// <summary>
/// A delegate that expands a query into zero or more alternative queries for processing.
/// </summary>
/// <param name="query">The original input query.</param>
/// <param name="searchContext">The context of the current search request.</param>
/// <returns>
/// A sequence of query variants to be processed. If no expansion is desired, return an empty or single-item list.
/// </returns>
/// <remarks>
/// Common use cases include synonym expansion, spelling alternatives, or multilingual equivalents.
/// </remarks>
public delegate IEnumerable<string> QueryExpander(string query, AutypoSearchContext searchContext);
