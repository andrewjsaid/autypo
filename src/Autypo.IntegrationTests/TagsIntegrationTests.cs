using Autypo.Configuration;
using Autypo.Tokenization;
using Shouldly;

namespace Autypo.IntegrationTests;

public class TagsIntegrationTests
{
    [Fact]
    public async Task When_tokens_are_tagged_then_they_make_their_way_to_matches()
    {
        var completer = await AutypoFactory.CreateSearchAsync<string>(config => config
            .WithDataSource(["aaa bbb"])
            .WithIndex(s => s, index => index
                .WithCandidateTagger((candidate, context) =>
                {
                    for (int i = 0; i < context.TransformedQueryTokens.Count; i++)
                    {
                        AutypoToken? token = context.TransformedQueryTokens[i];
                        foreach (var tag in token.Tags.Values)
                        {
                            candidate.Tags.Set(i + "_" + tag.Key, tag.Value);
                        }
                    }
                })
                .WithTextAnalyzer(analyzer => analyzer
                    .UseTransformer(() => new TestTransformer()))));

        var tags = completer.Search("aaa bbb").Single().Tags.Values.ToArray();
        tags.Select(t => $"{t.Key}: {t.Value}").ShouldBe(["0_text: aaa", "1_text: bbb"]);
    }

    private class TestTransformer : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer)
        {
            token.Tags.Set("text", token.Text);
            consumer.Accept(token);
        }
    }

}
