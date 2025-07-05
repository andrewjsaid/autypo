namespace Autypo;

/// <summary>
/// A delegate that determines the desired <see cref="TokenOrdering"/> strategy
/// based on the context of the query.
/// </summary>
/// <param name="queryContext">
/// The query context, which provides access to tokens and metadata associated with the request.
/// </param>
/// <returns>
/// The selected <see cref="TokenOrdering"/> mode to apply during token matching.
/// </returns>
public delegate TokenOrdering TokenOrderingSelector(AutypoQueryContext queryContext);
