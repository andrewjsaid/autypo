using Autypo.Configuration;

namespace Autypo;

/// <summary>
/// Provides factory methods for constructing and initializing Autypo search and autocomplete.
/// This type supports both eager and deferred initialization, allowing fine-grained control over indexing lifecycle.
/// </summary>
public abstract class AutypoFactory
{
    /// <summary>
    /// Creates and configures an Autypo search for documents of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="configure">A delegate that configures the search using a builder pattern.</param>
    /// <param name="cancellationToken">A token that may be used to cancel initialization or indexing.</param>
    /// <typeparam name="T">The type of the documents to be indexed and searched.</typeparam>
    /// <returns>
    /// A fully constructed and possibly initialized <see cref="AutypoSearch{T}"/> instance, depending on the initialization mode.
    /// </returns>
    /// <remarks>
    /// This method immediately performs eager initialization if configured to do so.
    /// If using background initialization, it starts a background task for loading the index.
    ///
    /// Lazy (on-demand) initialization will occur when the first query is executed.
    /// </remarks>
    public static async Task<IAutypoSearch<T>> CreateSearchAsync<T>(Action<AutypoConfigurationBuilder<T>> configure, CancellationToken cancellationToken = default) where T : notnull
    {
        var factory = new AutypoFactory<T>();
        factory.Configure(configure);
        await factory.InitializeIfEager(cancellationToken);
        _ = Task.Run(async () => await factory.InitializeIfBackground(cancellationToken), cancellationToken);
        return factory.AutypoSearch;
    }

    /// <summary>
    /// Creates and configures an Autypo autocomplete for simple string documents.
    /// </summary>
    /// <param name="configure">A delegate that configures the autocomplete using a builder pattern.</param>
    /// <param name="cancellationToken">A token that may be used to cancel initialization or indexing.</param>
    /// <returns>
    /// A fully constructed <see cref="AutypoComplete"/> instance that wraps an underlying string-based search.
    /// </returns>
    /// <remarks>
    /// This is a shorthand entry point for typical autocomplete scenarios. It uses <c>string</c> as the document type.
    /// </remarks>
    public static async Task<IAutypoComplete> CreateCompleteAsync(Action<AutypoConfigurationBuilder<string>> configure, CancellationToken cancellationToken = default)
    {
        var search = await CreateSearchAsync(configure, cancellationToken);
        return new AutypoComplete(search);
    }
    

    /// <summary>
    /// Initializes the autypo search immediately if it is configured for eager initialization.
    /// </summary>
    /// <param name="cancellationToken">A token that may cancel the initialization process.</param>
    public abstract Task InitializeIfEager(CancellationToken cancellationToken);

    /// <summary>
    /// Begins background initialization if the engine is configured for background loading.
    /// </summary>
    /// <param name="cancellationToken">A token that may cancel the initialization process.</param>
    public abstract Task InitializeIfBackground(CancellationToken cancellationToken);
}

/// <summary>
/// A generic engine for constructing and initializing strongly-typed Autypo search.
/// </summary>
/// <typeparam name="T">The document type to be indexed and searched. Must be non-nullable.</typeparam>
public sealed class AutypoFactory<T> : AutypoFactory where T : notnull
{
    private AutypoSearch<T>? _autypoSearch;

    /// <summary>
    /// Applies user configuration and builds the engine’s internal search pipeline.
    /// Must be called before accessing <see cref="AutypoSearch"/>.
    /// </summary>
    /// <param name="configure">A delegate used to configure the engine.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the engine has already been configured.
    /// </exception>
    public void Configure(Action<AutypoConfigurationBuilder<T>> configure)
    {
        if (_autypoSearch is not null)
        {
            throw new InvalidOperationException(Resources.AutypoEngine_AutypoSearch_AlreadyConfigured);
        }

        var configBuilder = new AutypoConfigurationBuilder<T>();
        configure(configBuilder);
        var built = configBuilder.Build(null);

        var autypoSearch = new AutypoSearch<T>(built.Config);

        foreach (var refreshToken in built.RefreshTokens)
        {
            refreshToken.Register(autypoSearch);
        }

        _autypoSearch = autypoSearch;
    }

    /// <inheritdoc />
    public override async Task InitializeIfEager(CancellationToken cancellationToken)
    {
        if (_autypoSearch is { InitializationMode: InitializationMode.Eager } autypoSearch)
        {
            await autypoSearch.ReloadAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task InitializeIfBackground(CancellationToken cancellationToken)
    {
        if (_autypoSearch is { InitializationMode: InitializationMode.Background } autypoSearch)
        {
            await autypoSearch.ReloadAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Gets the constructed Autypo search instance. This property is only valid after <see cref="Configure"/> has been called.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the engine has not been configured yet.
    /// </exception>
    public IAutypoSearch<T> AutypoSearch => _autypoSearch ?? throw new InvalidOperationException(Resources.AutypoEngine_AutypoSearch_NotConfigured);
}
