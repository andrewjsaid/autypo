using Autypo.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Autypo.AspNetCore;

/// <summary>
/// Provides ASP.NET Core-specific extensions for configuring Autypo data sources using <see cref="IServiceProvider"/>.
/// </summary>
/// <remarks>
/// These overloads allow injecting services into your indexing pipeline using DI-aware factories.
///
/// <para>Example (Typed data source service):</para>
/// <code>
/// builder.WithDataSource(sp => sp.GetRequiredService&lt;MyCustomDataSource&gt;());
/// </code>
///
/// <para>Example (LINQ-wrapped static list):</para>
/// <code>
/// builder.WithDataSource(sp => new[] { "hello", "hi", "hey" });
/// </code>
/// </remarks>
public static class AutypoConfigurationBuilderAspNetCoreExtensions
{
    #region DataSource

    /// <summary>
    /// Registers a data source using an <see cref="IServiceProvider"/> to resolve an <see cref="IAutypoDataSource{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="this">The Autypo configuration builder.</param>
    /// <param name="dataSource">A factory method that receives an <see cref="IServiceProvider"/> and returns an Autypo data source.</param>
    /// <returns>The updated configuration builder.</returns>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, Func<IServiceProvider, IAutypoDataSource<T>> dataSource) where T : notnull
        => @this.WithDataSourceFactory(context =>
        {
            var serviceProvider = (IServiceProvider?)context ?? throw new InvalidOperationException(Resources.AutypoAspNetCoreConfiguration_MissingServiceProvider);
            return new DataSourceWrapper<T>(serviceProvider, async (sp, ct) =>
            {
                var innerSource = dataSource(sp);
                return await innerSource.LoadDocumentsAsync(ct);
            });
        });

    /// <summary>
    /// Registers a synchronous data source using an <see cref="IServiceProvider"/> to provide documents.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="this">The Autypo configuration builder.</param>
    /// <param name="dataSource">A factory that receives the service provider and returns an enumerable of documents.</param>
    /// <returns>The updated configuration builder.</returns>
    /// <remarks>
    /// This is a convenient overload for static or in-memory datasets.
    /// </remarks>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, Func<IServiceProvider, IEnumerable<T>> dataSource) where T : notnull
        => @this.WithDataSourceFactory(context =>
        {
            var serviceProvider = (IServiceProvider?)context ?? throw new InvalidOperationException(Resources.AutypoAspNetCoreConfiguration_MissingServiceProvider);
            return new DataSourceWrapper<T>(serviceProvider, (sp, ct) => Task.FromResult(dataSource(sp)));
        });

    /// Registers an asynchronous data source using an <see cref="IServiceProvider"/> to provide documents asynchronously.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="this">The Autypo configuration builder.</param>
    /// <param name="dataSource">A factory that receives the service provider and a cancellation token, and returns a task that resolves to the documents.</param>
    /// <returns>The updated configuration builder.</returns>
    /// <remarks>
    /// This overload is useful for scenarios where document loading is I/O-bound or database-backed.
    /// </remarks>
    public static AutypoConfigurationBuilder<T> WithDataSource<T>(this AutypoConfigurationBuilder<T> @this, Func<IServiceProvider, CancellationToken, Task<IEnumerable<T>>> dataSource) where T : notnull
        => @this.WithDataSourceFactory(context =>
        {
            var serviceProvider = (IServiceProvider?)context ?? throw new InvalidOperationException(Resources.AutypoAspNetCoreConfiguration_MissingServiceProvider);
            return new DataSourceWrapper<T>(serviceProvider, async (sp, ct) => await dataSource(sp, ct));
        });

    #endregion
}

internal class DataSourceWrapper<T>(
    IServiceProvider serviceProvider,
    Func<IServiceProvider, CancellationToken, Task<IEnumerable<T>>> source) : IAutypoDataSource<T>
{

    public async Task<IEnumerable<T>> LoadDocumentsAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        return await source(scope.ServiceProvider, cancellationToken);
    }
}
