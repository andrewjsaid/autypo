using System.Net.Http.Json;
using Autypo.AspNetCore;
using Autypo.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Autypo.IntegrationTests;

public class AspNetIntegrationTests
{
    [Fact]
    public async Task When_api_uses_eager_datasource()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddAutypoComplete(config => config.WithDataSource(["one", "two", "three"]));

        var app = builder.Build();

        app.MapGet("", ([FromQuery] string query, [FromServices] IAutypoComplete autoComplete) => autoComplete.Complete(query));

        await app.StartAsync();
        
        var client = app.GetTestClient();

        var results = await client.GetFromJsonAsync<string[]>("?query=thr");

        results.ShouldBe(["three"]);
    }

    [Fact]
    public async Task When_api_uses_background_datasource()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddAutypoComplete(config => config
                            .WithDataSource(["one", "two", "three"])
                            .WithBackgroundLoading(UninitializedBehavior.Throw));

        var app = builder.Build();

        app.MapGet("", async ([FromQuery] string query, [FromServices] IAutypoComplete autoComplete) => await autoComplete.CompleteAsync(query));

        await app.StartAsync();

        var client = app.GetTestClient();

        var results = await client.GetFromJsonAsync<string[]>("?query=thr");

        results.ShouldBe(["three"]);
    }

    [Fact]
    public async Task When_api_uses_lazy_datasource()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddAutypoComplete(config => config
                            .WithDataSource(["one", "two", "three"])
                            .WithLazyLoading(UninitializedBehavior.Throw));

        var app = builder.Build();

        app.MapGet("", async ([FromQuery] string query, [FromServices] IAutypoComplete autoComplete) => await autoComplete.CompleteAsync(query));

        await app.StartAsync();

        var client = app.GetTestClient();

        var results = await client.GetFromJsonAsync<string[]>("?query=thr");

        results.ShouldBe(["three"]);
    }

    [Fact]
    public async Task When_api_uses_injected_datasource()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton<string[]>(["one", "two", "three"]);
        builder.Services.AddAutypoComplete(config => config.WithDataSource(sp => sp.GetRequiredService<string[]>()));

        var app = builder.Build();

        app.MapGet("", ([FromQuery] string query, [FromServices] IAutypoComplete autoComplete) => autoComplete.Complete(query));

        await app.StartAsync();

        var client = app.GetTestClient();

        var results = await client.GetFromJsonAsync<string[]>("?query=thr");

        results.ShouldBe(["three"]);
    }
}
