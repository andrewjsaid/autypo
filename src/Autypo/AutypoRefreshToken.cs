namespace Autypo;

/// <summary>
/// A token used to refresh one or more associated Autypo search instances.
/// </summary>
/// <remarks>
/// This is used to trigger a reload (reindex) of all registered <see cref="AutypoSearch"/> instances
/// when new data is available or indexing needs to be refreshed programmatically.
/// </remarks>
public sealed class AutypoRefreshToken
{
    private readonly List<AutypoSearch> _autypoSearches = [];

    /// <summary>
    /// Triggers a reload of all registered <see cref="AutypoSearch"/> instances.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the reload operation.</param>
    /// <returns>A task that completes when all reload operations have finished.</returns>
    /// <remarks>
    /// This will reindex all associated Autypo search engines, even if a reload is already in progress.
    /// Use sparingly in high-throughput environments.
    /// </remarks>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        foreach (var autypoCompleter in _autypoSearches)
        {
            await autypoCompleter.TriggerReloadAsync(triggerAgainIfAlreadyRunning: true, cancellationToken);
        }
    }

    /// <summary>
    /// Registers an <see cref="AutypoSearch"/> instance to be refreshed when this token is triggered.
    /// </summary>
    /// <param name="autypoSearch">The Autypo search instance to register.</param>
    /// <remarks>
    /// Each token can manage one or more Autypo engines. Multiple engines can share a token for coordinated refreshes.
    /// </remarks>
    public void Register<T>(IAutypoSearch<T> autypoSearch) where T : notnull
    {
        if (autypoSearch is AutypoSearch concrete)
        {
            _autypoSearches.Add(concrete);
        }
    }
}
