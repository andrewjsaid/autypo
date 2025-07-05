namespace Autypo;

/// <summary>
/// Represents a delegate that determines the maximum number of results to return for a search operation.
/// </summary>
/// <param name="searchContext">The context of the current search, including metadata and caller-defined parameters.</param>
/// <returns>
/// The maximum number of results to return. Return <c>null</c> for unbounded results.
/// Returning <c>0</c> will cause the engine to skip scoring and return no results.
/// </returns>
/// <remarks>
/// This delegate is useful for customizing result limits based on the caller’s context, user role, or query intent.
/// </remarks>
public delegate int? MaxResultsSelector(AutypoSearchContext searchContext);
