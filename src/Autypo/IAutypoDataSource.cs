namespace Autypo;

/// <summary>
/// Defines a contract for providing documents to an Autypo search engine instance.
/// </summary>
/// <typeparam name="T">The type of documents being indexed.</typeparam>
public interface IAutypoDataSource<T>
{
    /// <summary>
    /// Asynchronously loads documents to be indexed by Autypo.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of documents to be indexed.</returns>
    /// <remarks>
    /// This method is called during initialization or reload of the search index.
    /// Implementations may fetch data from in-memory collections, databases, APIs, or any other source.
    /// </remarks>
    Task<IEnumerable<T>> LoadDocumentsAsync(CancellationToken cancellationToken);
}
