namespace Autypo;

/// <summary>
/// A delegate that supplies fallback results when a query is empty (e.g., no user input).
/// </summary>
/// <typeparam name="T">The type of the documents returned by the autocomplete system.</typeparam>
/// <param name="queryContext">
/// The context of the current query, including tokenized data, metadata, and index information.
/// This allows the handler to make context-aware decisions, such as user-specific fallbacks or dynamic default suggestions.
/// </param>
/// <returns>
/// A sequence of documents of type <typeparamref name="T"/> to return in place of search results when the query is empty.
/// </returns>
public delegate IEnumerable<T> EmptyQueryHandler<out T>(AutypoQueryContext queryContext);
