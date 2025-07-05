using Autypo.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Autypo.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Autypo search services for the specified document type in the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">A configuration action to define the search behavior and indexing rules.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This registers:
    /// <list type="bullet">
    ///   <item><description><see cref="IAutypoSearch{T}"/></description></item>
    ///   <item><description><see cref="IAutypoRefresh{T}"/></description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddAutypoSearch<T>(this IServiceCollection services, Action<AutypoConfigurationBuilder<T>> configure) where T : notnull
    {
        services.AddSingleton<AutypoFactory<T>>(sp =>
        {
            var factory = new AutypoFactory<T>();
            factory.Configure(config =>
            {
                config.WithDataSourceContext(sp);
                configure(config);
            });
            return factory;
        });

        services.AddSingleton<IAutypoSearch<T>>(static sp => sp.GetRequiredService<AutypoFactory<T>>().AutypoSearch);
        services.AddSingleton<IAutypoRefresh<T>>(static sp =>
        {
            var token = new AutypoRefreshToken();
            token.Register(sp.GetRequiredService<IAutypoSearch<T>>());
            return new AutypoRefresh<T>(token);
        });

        // Hosted service needs unkeyed and untyped AutypoFactory
        services.AddSingleton<AutypoFactory>(static sp => sp.GetRequiredService<AutypoFactory<T>>());

        // AddHostedService only adds the service once even if called multiple times.
        services.AddHostedService<AutypoInitializationService>();

        return services;
    }

    /// <summary>
    /// Registers a named (keyed) Autypo search services for the specified document type in the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="key">A non-empty unique key for this instance.</param>
    /// <param name="configure">A configuration action to define the search behavior and indexing rules.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or whitespace.</exception>
    /// <remarks>
    /// This registers:
    /// <list type="bullet">
    ///   <item><description>keyed <see cref="IAutypoSearch{T}"/></description></item>
    ///   <item><description>keyed <see cref="IAutypoRefresh{T}"/></description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddKeyedAutypoSearch<T>(this IServiceCollection services, string key, Action<AutypoConfigurationBuilder<T>> configure) where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        services.AddKeyedSingleton<AutypoFactory<T>>(key, (sp, _) =>
        {
            var factory = new AutypoFactory<T>();
            factory.Configure(config =>
            {
                config.WithDataSourceContext(sp);
                configure(config);
            });
            return factory;
        });

        services.AddKeyedSingleton<IAutypoSearch<T>>(key, static (sp, key) => sp.GetRequiredKeyedService<AutypoFactory<T>>(key).AutypoSearch);
        services.AddKeyedSingleton<IAutypoRefresh<T>>(key, static (sp, key) =>
        {
            var token = new AutypoRefreshToken();
            token.Register(sp.GetRequiredKeyedService<IAutypoSearch<T>>(key));
            return new AutypoRefresh<T>(token);
        });

        // Hosted service needs unkeyed and untyped AutypoFactory
        services.AddSingleton<AutypoFactory>(sp => sp.GetRequiredKeyedService<AutypoFactory<T>>(key));

        // AddHostedService only adds the service once even if called multiple times.
        services.AddHostedService<AutypoInitializationService>();

        return services;
    }

    /// <summary>
    /// Registers Autypo autocomplete services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">A configuration action for the autocomplete index.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This is a specialization for string-based autocomplete and registers:
    /// <list type="bullet">
    ///   <item><description><see cref="IAutypoComplete"/></description></item>
    ///   <item><description><see cref="IAutypoRefresh"/></description></item>
    ///   <item><description>Background service for initialization</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddAutypoComplete(this IServiceCollection services, Action<AutypoConfigurationBuilder<string>> configure)
    {
        services.AddSingleton<AutoCompleteAutypoFactory>(sp =>
        {
            var factory = new AutypoFactory<string>();
            factory.Configure(config =>
            {
                config.WithDataSourceContext(sp);
                configure(config);
            });
            return new AutoCompleteAutypoFactory(factory);
        });

        services.AddSingleton<IAutypoComplete>(static sp => IAutypoComplete.Create(sp.GetRequiredService<AutoCompleteAutypoFactory>().AutypoFactory.AutypoSearch));
        services.AddSingleton<IAutypoRefresh>(static sp =>
        {
            var token = new AutypoRefreshToken();
            token.Register(sp.GetRequiredService<AutoCompleteAutypoFactory>().AutypoFactory.AutypoSearch);
            return new AutypoRefresh(token);
        });

        // Hosted service needs unkeyed and untyped AutypoFactory
        services.AddSingleton<AutypoFactory>(static sp => sp.GetRequiredService<AutoCompleteAutypoFactory>().AutypoFactory);

        // AddHostedService only adds the service once even if called multiple times.
        services.AddHostedService<AutypoInitializationService>();

        return services;
    }

    /// <summary>
    /// Registers a named (keyed)  Autypo autocomplete services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="key">A non-empty unique key for the autocomplete instance.</param>
    /// <param name="configure">A configuration action for the autocomplete index.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This is a specialization for string-based autocomplete and registers:
    /// <list type="bullet">
    ///   <item><description>keyed <see cref="IAutypoComplete"/></description></item>
    ///   <item><description>keyed <see cref="IAutypoRefresh"/></description></item>
    ///   <item><description>Background service for initialization</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddKeyedAutypoComplete(this IServiceCollection services, string key, Action<AutypoConfigurationBuilder<string>> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        services.AddKeyedSingleton<AutoCompleteAutypoFactory>(key, (sp, _) =>
        {
            var factory = new AutypoFactory<string>();
            factory.Configure(config =>
            {
                config.WithDataSourceContext(sp);
                configure(config);
            });
            return new AutoCompleteAutypoFactory(factory);
        });

        services.AddKeyedSingleton<IAutypoComplete>(key, static (sp, key) => IAutypoComplete.Create(sp.GetRequiredKeyedService<AutoCompleteAutypoFactory>(key).AutypoFactory.AutypoSearch));
        services.AddKeyedSingleton<IAutypoRefresh>(key, static (sp, key) =>
        {
            var token = new AutypoRefreshToken();
            token.Register(sp.GetRequiredKeyedService<AutoCompleteAutypoFactory>(key).AutypoFactory.AutypoSearch);
            return new AutypoRefresh(token);
        });

        // Hosted service needs unkeyed and untyped AutypoFactory
        services.AddSingleton<AutypoFactory>(sp => sp.GetRequiredKeyedService<AutoCompleteAutypoFactory>(key).AutypoFactory);

        // AddHostedService only adds the service once even if called multiple times.
        services.AddHostedService<AutypoInitializationService>();

        return services;

    }

    private class AutoCompleteAutypoFactory(AutypoFactory<string> factory)
    {
        public AutypoFactory<string> AutypoFactory { get; } = factory;
    }
}
