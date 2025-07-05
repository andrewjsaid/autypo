namespace Autypo;

/// <summary>
/// Provides common reusable implementations of <see cref="QueryFilter"/>.
/// </summary>
internal static class QueryFilters
{
    /// <summary>
    /// Returns a <see cref="QueryFilter"/> that accepts queries of at least the specified minimum length.
    /// </summary>
    /// <param name="length">The minimum number of characters required for a query to be accepted.</param>
    /// <returns>
    /// A <see cref="QueryFilter"/> that returns <c>true</c> if the query length is greater than or equal to <paramref name="length"/>.
    /// </returns>
    public static QueryFilter MinimumLength(int length)
        => (query, _) => query.Length >= length;

    /// <summary>
    /// Returns a <see cref="QueryFilter"/> that requires all of the given filters to pass for the query to be accepted.
    /// </summary>
    /// <param name="filters">An array of filters to combine with logical AND.</param>
    /// <returns>
    /// A <see cref="QueryFilter"/> that returns <c>true</c> only if all provided filters return <c>true</c>.
    /// </returns>
    public static QueryFilter All(QueryFilter[] filters)
        => (query, context) =>
            {
                foreach (var filter in filters)
                {
                    if (!filter(query, context))
                    {
                        return false;
                    }
                }

                return true;
            };
}
