using Autypo.Configuration;
using Shouldly;

namespace Autypo.UnitTests;

public class AutypoFactoryTests
{
    [Fact]
    public async Task Start()
    {
        var qString = await AutypoFactory.CreateSearchAsync<TestItem>(config => config
                    .WithIndex(k => k.Name)
                    .WithDataSource([new TestItem("quick brown fox")]));

        qString.Search("quick").Select(r => r.Value.Name).ShouldBe(["quick brown fox"]);
    }

    private record TestItem(string Name);
}
