using Autypo.Configuration;
using Shouldly;

namespace Autypo.UnitTests;

public class AutypoCompleterExtensionsTests
{
    [Fact]
    public async Task SearchAsync_loads_data()
    {
        var q = await AutypoFactory.CreateCompleteAsync(index => index
                    .WithLazyLoading(onUninitialized: UninitializedBehavior.ReturnEmpty)
                    .WithDataSource(["apple"]));
        
        q.Complete("apple").Count().ShouldBe(0);

        (await q.CompleteAsync("apple")).Count().ShouldBe(1);

        q.Complete("apple").Count().ShouldBe(1);
    }
}
