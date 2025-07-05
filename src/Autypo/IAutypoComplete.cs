namespace Autypo;

/// <summary>
/// Defines an interface for providing autocomplete suggestions based on partial user input.
/// </summary>
public interface IAutypoComplete
{
    /// <summary>
    /// Retrieves a list of completion suggestions for the given partial term.
    /// </summary>
    /// <param name="term">The partial query input for which suggestions should be generated.</param>
    /// <param name="context">
    /// Optional context containing metadata or other environmental state that may influence suggestion behavior.
    /// </param>
    /// <returns>A sequence of suggestion strings relevant to the input term.</returns>
    /// <remarks>
    /// Completion internally performs a query to gather candidates. If Autypo is configured for background or lazy
    /// initialization and the index is not yet loaded, this method may throw or an empty result.
    /// </remarks>
    IEnumerable<string> Complete(string term, AutypoSearchContext? context = null);


    /// <summary>
    /// Asynchronously retrieves a list of completion suggestions for the given partial term.
    /// </summary>
    /// <param name="term">The partial query input for which suggestions should be generated.</param>
    /// <param name="context">
    /// Optional context containing metadata or other environmental state that may influence suggestion behavior.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that produces a sequence of suggestion strings relevant to the input term.</returns>
    /// <remarks>
    /// If the underlying index is not yet initialized (and Autypo is using background or lazy loading),
    /// this method will transparently trigger the index load before returning completions.
    /// </remarks>
    ValueTask<IEnumerable<string>> CompleteAsync(string term, AutypoSearchContext? context = null, CancellationToken cancellationToken = default);

    public static IAutypoComplete Create(IAutypoSearch<string> autypoSearch) => new AutypoComplete(autypoSearch);
}
