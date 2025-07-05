namespace Autypo;

/// <summary>
/// Provides a strongly typed interface for performing document search operations within Autypo.
/// </summary>
/// <typeparam name="T">The type of the document being searched. Must be non-nullable.</typeparam>
public interface IAutypoSearch<T> where T : notnull
{
    /// <summary>
    /// Executes a search against the configured indices using the given query string and optional search context.
    /// </summary>
    /// <param name="query">The query string to search for. Must not be <c>null</c>.</param>
    /// <param name="context">
    /// Optional context providing metadata or environmental information that may influence filtering,
    /// scoring, or tokenization behavior.
    /// </param>
    /// <returns>
    /// A sequence of search results, each containing a matched document and any associated metadata tags.
    /// </returns>
    /// <remarks>
    /// If Autypo is configured for background or lazy initialization, and initialization has not yet completed,
    /// this method will return fallback results provided by the <c>UninitializedDataSourceHandler</c> (if configured),
    /// or an empty result set otherwise.
    /// </remarks>
    IEnumerable<AutypoSearchResult<T>> Search(string query, AutypoSearchContext? context = null);

    /// <summary>
    /// Asynchronously executes a search against the configured indices using the given query string and optional context.
    /// </summary>
    /// <param name="term">The query string to search for. Must not be <c>null</c>.</param>
    /// <param name="context">
    /// Optional context providing metadata or environmental information that may influence filtering,
    /// scoring, or tokenization behavior.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to abort the operation.</param>
    /// <returns>
    /// A task that produces a sequence of search results, each containing a matched document and any associated metadata tags.
    /// </returns>
    /// <remarks>
    /// If Autypo is still initializing and configured for lazy or background loading,
    /// this method will transparently trigger an index load before returning results.
    /// </remarks>
    ValueTask<IEnumerable<AutypoSearchResult<T>>> SearchAsync(string query, AutypoSearchContext? context = null, CancellationToken cancellationToken = default);
}
