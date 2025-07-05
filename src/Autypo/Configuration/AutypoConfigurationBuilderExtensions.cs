namespace Autypo.Configuration;

/// <summary>
/// Provides extension methods to simplify configuration of Autypo index builders.
/// </summary>
public static class AutypoConfigurationBuilderExtensions
{

    #region DataSource

    /// <summary>
    /// Configures the data source to use a pre-existing <see cref="IAutypoDataSource{T}"/> instance.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="dataSource">The data source instance.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, IAutypoDataSource<T> dataSource) where T : notnull
        => @this.WithDataSourceFactory(_ => dataSource);

    /// <summary>
    /// Configures the data source using a static collection of documents.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="documents">The documents to index.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, IEnumerable<T> documents) where T : notnull
        => @this.WithDataSourceFactory(_ => new DelegateDataSource<T>(_ => Task.FromResult(documents)));

    /// <summary>
    /// Configures the data source using a delegate that returns a document collection at runtime.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="dataSource">A delegate that returns the current set of documents.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, Func<IEnumerable<T>> dataSource) where T : notnull 
        => @this.WithDataSourceFactory(_ => new DelegateDataSource<T>(_ => Task.FromResult(dataSource())));

    /// <summary>
    /// Configures the data source using an asynchronous delegate that returns documents with support for cancellation.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="dataSource">An async delegate that retrieves the documents to index.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, Func<CancellationToken, Task<IEnumerable<T>>> dataSource) where T : notnull
        => @this.WithDataSourceFactory(_ => new DelegateDataSource<T>(dataSource));

    #endregion

    #region Loading

    /// <summary>
    /// Configures the index to load its data source eagerly at startup.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithEagerLoading<T>(this AutypoConfigurationBuilder<T> @this) where T : notnull
        => @this.WithInitializationMode(InitializationMode.Eager, default);

    /// <summary>
    /// Configures the index to load its data source asynchronously in the background after startup.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="onUninitialized">Specifies what should happen if a search occurs before initialization completes.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithBackgroundLoading<T>(this AutypoConfigurationBuilder<T> @this, UninitializedBehavior onUninitialized) where T : notnull
        => @this.WithInitializationMode(InitializationMode.Background, onUninitialized);

    /// <summary>
    /// Configures the index to load its data source lazily upon first query.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="onUninitialized">Specifies what should happen if a search occurs before initialization completes.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithLazyLoading<T>(this AutypoConfigurationBuilder<T> @this, UninitializedBehavior onUninitialized) where T : notnull
        => @this.WithInitializationMode(InitializationMode.Lazy, onUninitialized);

    #endregion

    #region Index

    /// <summary>
    /// Adds a simple index for the given key selector, using default tokenization settings.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="keySelector">A function that extracts a key from each document. Keys do not need to be unique.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithIndex<T>(this AutypoConfigurationBuilder<T> @this, Func<T, string?> keySelector) where T : notnull
        => @this.WithIndex(keySelector, static _ => { });

    #endregion

    #region MaxResults

    /// <summary>
    /// Limits the result set to a single top-ranked match.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithSingleResult<T>(this AutypoConfigurationBuilder<T> @this) where T : notnull
        => @this.WithMaxResults(static _ => 1);

    /// <summary>
    /// Sets a fixed maximum number of results to return per query.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <param name="maxResults">The maximum number of results to return.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithMaxResults<T>(this AutypoConfigurationBuilder<T> @this, int maxResults) where T : notnull 
        => @this.WithMaxResults(_ => maxResults);

    /// <summary>
    /// Disables result count limits and returns all matches.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    public static AutypoConfigurationBuilder<T> WithUnlimitedResults<T>(this AutypoConfigurationBuilder<T> @this) where T : notnull
        => @this.WithMaxResults(static _ => null);

    #endregion

    #region EmptyQueryHandler

    /// <summary>
    /// Configures the engine to return no results when the query string is empty.
    /// </summary>
    /// <param name="this">The builder to configure.</param>
    /// <typeparam name="T">The type of document being indexed.</typeparam>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This is the default behavior if no <see cref="WithEmptyQueryHandling"/> is specified.
    /// Note that Autypo does not support <c>null</c> queries—only empty strings trigger this handler.
    /// </remarks>
    public static AutypoConfigurationBuilder<T> WithNoResultsForEmptyQuery<T>(this AutypoConfigurationBuilder<T> @this) where T : notnull 
        => @this.WithEmptyQueryHandling(static _ => []);

    #endregion

}
