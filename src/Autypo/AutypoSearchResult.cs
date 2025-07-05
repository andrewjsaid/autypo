namespace Autypo;

/// <summary>
/// Represents the result of a search operation within Autypo, containing a matched document and any associated tags.
/// </summary>
/// <typeparam name="T">The type of the document returned by the search. Must be non-nullable.</typeparam>
public readonly struct AutypoSearchResult<T>(T value, AutypoTags tags) where T : notnull
{
    /// <summary>
    /// Gets the document that matched the search query.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    /// Gets the set of tags assigned to the result during ranking or filtering.
    /// These may be used to convey metadata or post-processing flags (e.g., from custom candidate taggers).
    /// </summary>
    public AutypoTags Tags { get; } = tags;
}
