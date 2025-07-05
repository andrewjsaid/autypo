namespace Autypo;

/// <summary>
/// Wraps a delegate-based document source for Autypo indexing.
/// </summary>
/// <typeparam name="T">The type of the documents to load.</typeparam>
internal class DelegateDataSource<T>(Func<CancellationToken, Task<IEnumerable<T>>> source) : IAutypoDataSource<T>
{
    /// <summary>
    /// Loads the documents asynchronously by invoking the provided delegate.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, with the loaded documents.</returns>
    public Task<IEnumerable<T>> LoadDocumentsAsync(CancellationToken cancellationToken)
    {
        return source(cancellationToken);
    }
}
