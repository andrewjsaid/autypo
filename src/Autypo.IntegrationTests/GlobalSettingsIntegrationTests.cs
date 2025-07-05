using Autypo.Configuration;
using Shouldly;

namespace Autypo.IntegrationTests;

/// <summary>
/// Tests which check the various global toggles.
/// </summary>
public class GlobalSettingsIntegrationTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    public async Task When_max_results_are_bounded(int maxResults, int actualResults)
    {

        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaa 1", "aaa 2", "aaa 3"])
            .WithMaxResults(maxResults));

        completer.Complete("aaa").Count().ShouldBe(actualResults);
    }

    [Fact]
    public async Task When_should_index_is_set()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["yyaaaa", "ynaaaa", "nnaaaa", "nyaaaa"])
            .WithShouldIndex(d => d[0] == 'y')
            .WithIndex(d => d, index => index
                .WithNoFuzziness()
                .WithShouldIndex(d => d[1] == 'y')
                .WithCandidateTagger((_, context) =>
                {
                    context.DocumentCount.ShouldBe(2);
                    context.IndexedDocumentCount.ShouldBe(1);
                }))
            .WithUnlimitedResults());

        completer.Complete("yyaaaa").Count().ShouldBe(1);
        completer.Complete("ynaaaa").Count().ShouldBe(0);
        completer.Complete("nnaaaa").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_docs_are_scored()
    {
        var completer = await AutypoFactory.CreateSearchAsync<string>(config => config
            .WithDataSource(["aaa3", "aaa2", "aaa1", "aaa4"])
            .WithDocumentScorer(d => d[3] - '0')
            .WithIndex(d => d, index => index
                .WithNoFuzziness()
                .WithCandidateTagger((candidate, _) =>
                {
                    candidate.Tags.Set("docscore", candidate.DocumentScore);
                }))
            .WithUnlimitedResults());

        var searchResults = completer.Search("aaa").ToArray();
        searchResults.Length.ShouldBe(4);

        searchResults[0].Value.ShouldBe("aaa4");
        searchResults[0].Tags.TryGetValue("docscore", out var docScore).ShouldBeTrue();
        docScore.ShouldBe(4);

        searchResults[1].Value.ShouldBe("aaa3");
        searchResults[1].Tags.TryGetValue("docscore", out docScore).ShouldBeTrue();
        docScore.ShouldBe(3);

        searchResults[2].Value.ShouldBe("aaa2");
        searchResults[2].Tags.TryGetValue("docscore", out docScore).ShouldBeTrue();
        docScore.ShouldBe(2);

        searchResults[3].Value.ShouldBe("aaa1");
        searchResults[3].Tags.TryGetValue("docscore", out docScore).ShouldBeTrue();
        docScore.ShouldBe(1);
    }

    [Fact]
    public async Task When_empty_query()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"]));

        completer.Complete("").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_empty_query_custom()
    {
        var completer = await AutypoFactory.CreateSearchAsync<string>(config => config
            .WithDataSource(["aaaaa"])
            .WithEmptyQueryHandling(q => q.GetDocuments<string>().Take(1).Select(x => x.Document)));

        completer.Search("").Single().Value.ShouldBe("aaaaa");
    }

    [Fact]
    public async Task When_uninitialized()
    {
        var returnEmptyCompleter = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithLazyLoading(UninitializedBehavior.ReturnEmpty));

        var throwCompleter = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithLazyLoading(UninitializedBehavior.Throw));

        returnEmptyCompleter.Complete("aaaaa").Count().ShouldBe(0);
        (await returnEmptyCompleter.CompleteAsync("aaaaa")).Count().ShouldBe(1);

        Assert.Throws<InvalidOperationException>(() => throwCompleter.Complete("aaaaa"));
        (await throwCompleter.CompleteAsync("aaaaa")).Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_index_has_additional_keys()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithAdditionalKeys(s => [s + "1"])));

        completer.Complete("aaaaa").Count().ShouldBe(1);
        completer.Complete("aaaaa1").Count().ShouldBe(1);
        completer.Complete("aaaaa2").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_query_is_rewritten()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["abcdef"])
            .WithIndex(s => s, index => index
                .WithNoFuzziness()
                .WithFinalTokenFullMatchScope()
                .WithQueryExpander(static (query, context) => [query + context.Metadata?["suffix"]])
                .WithCandidateTagger(static (_, context) =>
                {
                    context.Query.ShouldBe(context.RawQuery + context.Metadata.GetValueOrDefault("suffix"));
                })));

        completer.Complete("abc").Count().ShouldBe(0);
        completer.Complete("abcdef").Count().ShouldBe(1);
        completer.Complete("abc", new AutypoSearchContext { Metadata = new Dictionary<string, object> { { "suffix", "def" } } }).Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_candidates_are_filtered()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaa1", "aaaa2", "aaaa3"])
            .WithIndex(s => s, index => index
                .WithCandidateFilter(static (candidate, context) => ((string[])context.Metadata["exclude"]).All(e => e != candidate.Document))));

        completer.Complete("aaaa", new AutypoSearchContext { Metadata = new Dictionary<string, object> { { "exclude", Array.Empty<string>() } } }).Count().ShouldBe(3);
        completer.Complete("aaaa", new AutypoSearchContext { Metadata = new Dictionary<string, object> { { "exclude", new[] { "aaaa2" } } } }).Count().ShouldBe(2);
    }
}