using Autypo.Configuration;
using Shouldly;

namespace Autypo.IntegrationTests;

public class LargeTextIntegrationTests
{
    [Fact]
    public async Task When_a_large_document_exists_then_the_first_64_tokens_match()
    {
        var tokens = Enumerable.Range(0, 10).SelectMany(i => Enumerable.Range(0, 10).Select(j => $"aaa{i}{j}")).ToArray();
        var longText = string.Join(" ", tokens);

        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([longText])
            .WithIndex(s => s, index => index.WithNoFuzziness())
            .WithUnlimitedResults());

        for (int i = 0; i < tokens.Length; i++)
        {
            completer.Complete(tokens[i]).Count().ShouldBe(i < 64 ? 1 : 0);
        }
    }

    [Fact]
    public async Task When_a_query_is_very_large_then_the_first_64_tokens_match()
    {
        var tokens = Enumerable.Range(0, 10).SelectMany(i => Enumerable.Range(0, 10).Select(j => $"aaa{i}{j}")).ToArray();
        var longText = string.Join(" ", tokens);

        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(tokens)
            .WithIndex(s => s, index => index.WithNoFuzziness().WithPartialTokenMatching().WithUnorderedTokenOrdering())
            .WithUnlimitedResults());

        completer.Complete(longText).ShouldBe(tokens[..64], ignoreOrder: true);
    }

}
