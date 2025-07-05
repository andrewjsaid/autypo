using Autypo.AspNetCore;
using Autypo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Autypo.UnitTests;

public class ServiceCollectionExtensionsTests
{

    [Fact]
    public void Single_completer_unkeyed()
    {
        var services = new ServiceCollection();
        services.AddAutypoComplete(config => config
            .WithDataSource(["Malta", "Great Britain"]));

        var serviceProvider = services.BuildServiceProvider();
        
        serviceProvider.GetService<IAutypoComplete>().ShouldNotBeNull();
        serviceProvider.GetService<IAutypoRefresh>().ShouldNotBeNull();

        serviceProvider.GetService<IAutypoSearch<string>>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh<string>>().ShouldBeNull();
    }

    [Fact]
    public void Single_completer_keyed()
    {
        var services = new ServiceCollection();
        services.AddKeyedAutypoComplete("countries", config => config
            .WithDataSource(["Malta", "Great Britain"]));

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetKeyedService<IAutypoComplete>("countries").ShouldNotBeNull();        
        serviceProvider.GetKeyedService<IAutypoRefresh>("countries").ShouldNotBeNull();

        serviceProvider.GetKeyedService<IAutypoSearch<string>>("countries").ShouldBeNull();
        serviceProvider.GetKeyedService<IAutypoRefresh<string>>("countries").ShouldBeNull(); 

        serviceProvider.GetService<IAutypoComplete>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh>().ShouldBeNull();

        serviceProvider.GetService<IAutypoSearch<string>>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh<string>>().ShouldBeNull();
    }

    [Fact]
    public void Single_search_unkeyed()
    {
        var services = new ServiceCollection();
        services.AddAutypoSearch<string>(config => config
            .WithDataSource(["Malta", "Great Britain"]));

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<IAutypoComplete>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh<IAutypoComplete>>().ShouldBeNull();

        serviceProvider.GetService<IAutypoSearch<string>>().ShouldNotBeNull();
        serviceProvider.GetService<IAutypoRefresh<string>>().ShouldNotBeNull();
    }

    [Fact]
    public void Single_search_keyed()
    {
        var services = new ServiceCollection();
        services.AddKeyedAutypoSearch<string>("countries", config => config
            .WithDataSource(["Malta", "Great Britain"]));

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetKeyedService<IAutypoComplete>("countries").ShouldBeNull();
        serviceProvider.GetKeyedService<IAutypoRefresh<IAutypoComplete>>("countries").ShouldBeNull();

        serviceProvider.GetKeyedService<IAutypoSearch<string>>("countries").ShouldNotBeNull();
        serviceProvider.GetKeyedService<IAutypoRefresh<string>>("countries").ShouldNotBeNull();

        serviceProvider.GetService<IAutypoComplete>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh<IAutypoComplete>>().ShouldBeNull();

        serviceProvider.GetService<IAutypoSearch<string>>().ShouldBeNull();
        serviceProvider.GetService<IAutypoRefresh<string>>().ShouldBeNull();
    }
}
