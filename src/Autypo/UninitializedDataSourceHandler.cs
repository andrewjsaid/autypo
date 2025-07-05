namespace Autypo;

/// <summary>
/// A delegate that provides fallback search results when the underlying index is not yet initialized.
/// </summary>
/// <typeparam name="T">The type of document stored in the index.</typeparam>
/// <param name="term">
/// The user-provided search term. May be empty or incomplete depending on UI interaction timing.
/// </param>
/// <param name="searchContext">
/// The current <see cref="AutypoSearchContext"/> providing request metadata and context-specific state.
/// </param>
/// <returns>
/// A sequence of <see cref="AutypoSearchResult{T}"/> to return in lieu of executing an actual search.
/// </returns>
/// <remarks>
/// This delegate is only invoked if a synchronous search is attempted before data is ready,
/// and a custom fallback strategy is configured.
/// </remarks>
internal delegate IEnumerable<AutypoSearchResult<T>> UninitializedDataSourceHandler<T>(string term, AutypoSearchContext searchContext) where T : notnull;
