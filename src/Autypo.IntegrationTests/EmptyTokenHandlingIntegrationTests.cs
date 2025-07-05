using Autypo.Configuration;
using Autypo.Tokenization;
using Shouldly;

namespace Autypo.IntegrationTests;

public class EmptyTokenHandlingIntegrationTests
{
    [Fact]
    public async Task When_document_has_only_whitespace_then_it_does_not_cause_errors()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaa", "   ", "bbb"]));


        completer.Complete("aaa").Single().ShouldBe("aaa");
    }

    [Fact]
    public async Task When_document_has_no_tokens_then_it_does_not_cause_errors()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaa", "   ", "bbb"])
            .WithIndex(s => s, index => index
                .WithTextAnalyzer(analyzer => analyzer
                    .UseDocumentTransformer(() => new TestNoTokenizer()))));


        completer.Complete("aaa").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_query_has_no_tokens_then_it_does_not_cause_errors()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaa", "bbb"])
            .WithIndex(s => s, index => index
                .WithTextAnalyzer(analyzer => analyzer
                    .UseQueryTransformer(() => new TestNoTokenizer()))));


        completer.Complete("aaa").Count().ShouldBe(0);
    }

    private class TestNoTokenizer : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer) { }
    }

}
