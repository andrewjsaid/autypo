using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class AutypoTokenTaggingTests
{

    [Fact]
    public void When_token_is_tagged_then_tag_is_stored()
    {
        var token = new AutypoToken(0, 5, "12345".AsMemory(), AutypoTags.None);

        token.Tags.Set("stop", true);

        token.Tags.TryGetValue("stop", out bool stop).ShouldBeTrue();
        stop.ShouldBeTrue();
    }

}
