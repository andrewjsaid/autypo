using Autypo.AspNetCore;
using Autypo.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Autypo.UnitTests;

public class AutypoRefreshTokenTests
{
    [Fact]
    public async Task When_refresh_token_is_configured_specifically()
    {
        var refreshToken = new AutypoRefreshToken();

        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(new TestDataSource())
            .UseRefreshToken(refreshToken)
            .WithEmptyQueryHandling(static (context) =>
            {
                context.DocumentType.ShouldBe(typeof(string));
                return context.GetDocuments<string>().Select(d => d.Document).ToArray();
            }));

        completer.Complete("").ShouldBe(["0"]);
        completer.Complete("").ShouldBe(["0"]);

        await refreshToken.RefreshAsync();

        completer.Complete("").ShouldBe(["1"]);
    }

    [Fact]
    public async Task When_refresh_token_is_registered_in_DI()
    {
        var services = new ServiceCollection();

        services.AddAutypoSearch<string>(config => config
            .WithDataSource(new TestDataSource())
            .WithLazyLoading(onUninitialized: UninitializedBehavior.ReturnEmpty)
            .WithEmptyQueryHandling(static (context) => context.GetDocuments<string>().Select(d => d.Document).ToArray()));

        var serviceProvider = services.BuildServiceProvider();

        var completer = serviceProvider.GetRequiredService<IAutypoSearch<string>>();
        var refresher = serviceProvider.GetRequiredService<IAutypoRefresh<string>>();
        refresher.ShouldNotBeNull();

        (await completer.SearchAsync("")).Select(d => d.Value).ShouldBe(["0"]);
        (await completer.SearchAsync("")).Select(d => d.Value).ShouldBe(["0"]);

        await refresher.RefreshAsync();

        completer.Search("").Select(d => d.Value).ShouldBe(["1"]);
    }

    private class TestDataSource : IAutypoDataSource<string>
    {
        private int _counter;

        public Task<IEnumerable<string>> LoadDocumentsAsync(CancellationToken cancellationToken)
        {
            IEnumerable<string> results = [_counter++.ToString()];
            return Task.FromResult(results);
        }
    }
}
