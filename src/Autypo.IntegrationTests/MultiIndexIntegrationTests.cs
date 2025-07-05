using Autypo.Configuration;
using Shouldly;

namespace Autypo.IntegrationTests;

/// <summary>
/// Tests for scenarios with multiple indices.
/// </summary>
public class MultiIndexIntegrationTests
{

    [Fact]
    public async Task When_indexing_different_aspects()
    {
        var completer = await AutypoFactory.CreateSearchAsync<Country>(config => config
            .WithDataSource([
                new Country("ES", "Spain"),
                new Country("EE", "Estonia")
            ])
            .WithIndex(c => c.Code, index => index
                .WithCaseSensitivity(true)
                .WithNoFuzziness()
                .AddQueryFilter((q, _) => q.Length == 2)
                .WithName("ByCode")
                .WithPriority(1))
            .WithIndex(c => c.Name, index => index
                .AddNoMinimumLengthQueryFilter()
                .WithName("ByName")
                .WithPriority(0))
        );

        completer.Search("es").Single().Value.Code.ShouldBe("EE", "Code is case sensitive so we match by name");
        completer.Search("ES").Single().Value.Code.ShouldBe("ES", "Code wins priority so we ignore name");
        completer.Search("E").Single().Value.Code.ShouldBe("EE", "Code doesn't match we match by name");
        completer.Search("EST").Single().Value.Code.ShouldBe("EE", "Code doesn't match we match by name");
        completer.Search("SP").Single().Value.Code.ShouldBe("ES", "Code doesn't match we match by name");
    }

    private record Country(string Code, string Name);
}