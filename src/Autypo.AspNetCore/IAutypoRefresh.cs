namespace Autypo.AspNetCore;

/// <summary>
/// Represents a refresh control handle for a specific <see cref="IAutypoSearch{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the document being searched. Must be non-nullable.</typeparam>
public interface IAutypoRefresh<T> where T : notnull
{
    /// <summary>
    /// Triggers a reload (reindexing) of the associated Autypo engine and waits for the indexing to complete.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the reload.</param>
    /// <returns>A task that completes once the reload is finished.</returns>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a refresh control handle for a specific <see cref="IAutypoComplete"/>.
/// </summary>
public interface IAutypoRefresh
{
    /// <summary>
    /// Triggers a reload (reindexing) of the associated Autypo engine and waits for the indexing to complete.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the reload.</param>
    /// <returns>A task that completes once the reload is finished.</returns>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a refresh control handle for a specific <see cref="IAutypoSearch{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the document being searched. Must be non-nullable.</typeparam>
internal class AutypoRefresh<T>(AutypoRefreshToken refreshToken) : IAutypoRefresh<T> where T : notnull
{
    /// <inheritdoc />
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await refreshToken.RefreshAsync(cancellationToken);
    }
}

/// <summary>
/// Represents a refresh control handle for a specific <see cref="IAutypoComplete"/>.
/// </summary>
internal class AutypoRefresh(AutypoRefreshToken refreshToken) : IAutypoRefresh
{
    /// <inheritdoc />
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await refreshToken.RefreshAsync(cancellationToken);
    }
}
