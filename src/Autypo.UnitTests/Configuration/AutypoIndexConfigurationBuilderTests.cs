using Autypo.Configuration;
using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Configuration;

public class AutypoIndexConfigurationBuilderTests
{
    [Fact]
    public void When_build_with_defaults()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static key => key);

        var index = builder.Build();

        const string query = "123456789 123456 1234 12";
        var queryContext = CreateQueryContext(query);

        index.Name.ShouldBeNull();
        index.EnableCaseSensitivity.ShouldBeFalse();
        index.QueryExpander(query, new()).ShouldBe([query]);

        index.QueryFilter("q", new()).ShouldBeFalse(); // min length is 3 by default
        index.QueryFilter("qq", new()).ShouldBeFalse(); // min length is 3 by default
        index.QueryFilter("qqq", new()).ShouldBeTrue();
        index.QueryFilter("qqqq", new()).ShouldBeTrue();

        index.TokenOrderingSelector(queryContext).ShouldBe(TokenOrdering.InOrder);

        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[0], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 2, allowTransposition: true));
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[1], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 2, allowTransposition: false));
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[2], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 1, allowTransposition: false));
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[3], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 0, allowTransposition: false));

        index.MatchScopeSelector(queryContext.ExtractedQueryTokens[0], queryContext).ShouldBe(MatchScope.Full);
        index.MatchScopeSelector(queryContext.ExtractedQueryTokens[1], queryContext).ShouldBe(MatchScope.Full);
        index.MatchScopeSelector(queryContext.ExtractedQueryTokens[2], queryContext).ShouldBe(MatchScope.Full);
        index.MatchScopeSelector(queryContext.ExtractedQueryTokens[3], queryContext).ShouldBe(MatchScope.Prefix);

        index.QueryTextAnalyzerFactory().Analyze(query.AsMemory()).ExtractedTokens.Select(t => t.Text).ShouldBe(queryContext.ExtractedQueryTokens.Select(t => t.Text));
        index.DocumentTextAnalyzerFactory().Analyze(query.AsMemory()).ExtractedTokens.Select(t => t.Text).ShouldBe(queryContext.ExtractedQueryTokens.Select(t => t.Text));
        index.CalculatePartialTokenMatchingPolicy.ShouldBeNull();
        index.PrioritySelector(new()).ShouldBe(0);
        index.KeySelector(query).ShouldBe(query);
        index.AdditionalKeySelectors.ShouldBeNull();
        index.ShouldIndex.ShouldBeNull();
        // No real way to test index.CandidateScorer
        index.CandidateFilter.ShouldBeNull();
        index.CandidateTagger.ShouldBeNull();
    }

    [Fact]
    public void When_key_selector_modifies_string()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static key => key + "!");

        var index = builder.Build();

        index.KeySelector("query").ShouldBe("query!");
    }

    [Fact]
    public void When_key_selector_selects_property()
    {
        var builder = new AutypoIndexConfigurationBuilder<TestItem>(static item => item.Item1);

        var index = builder.Build();

        index.KeySelector(new TestItem("item1", "item2")).ShouldBe("item1");
    }

    [Fact]
    public void When_key_selector_has_additional_keys()
    {
        var builder = new AutypoIndexConfigurationBuilder<TestItem>(static item => item.Item1);

        builder.WithAdditionalKeys(static item => [item.Item1 + "!", item.Item2]);

        var index = builder.Build();

        index.KeySelector(new TestItem("item1", "item2")).ShouldBe("item1");
        index.AdditionalKeySelectors.ShouldNotBeNull();
        index.AdditionalKeySelectors(new TestItem("item1", "item2")).ShouldBe(["item1!", "item2"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_case_sensitivity_is_specified(bool value)
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithCaseSensitivity(value: value);

        var index = builder.Build();

        index.EnableCaseSensitivity.ShouldBe(value);
    }

    [Fact]
    public void When_query_expander_suppresses_query()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithQueryExpander(static (_, _) => []);

        var index = builder.Build();

        index.QueryExpander.ShouldNotBeNull();
        index.QueryExpander("query", new()).ShouldBeEmpty();
    }

    [Fact]
    public void When_query_expander_adds_value()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithQueryExpander(static (q, _) => [q, q + "!"]);

        var index = builder.Build();

        index.QueryExpander("query", new()).ShouldBe(["query", "query!"]);
    }

    [Fact]
    public void When_query_filter_is_specified_once()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.AddQueryFilter(static (q, _) => q.Length == 5);

        var index = builder.Build();

        index.QueryFilter("query", new()).ShouldBeTrue();
        index.QueryFilter("query2", new()).ShouldBeFalse();
    }

    [Fact]
    public void When_query_filter_is_specified_twice()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.AddQueryFilter(static (q, _) => q.Length == 5);
        builder.AddQueryFilter(static (q, _) => q[0] == 'q');

        var index = builder.Build();

        index.QueryFilter("query", new()).ShouldBeTrue();
        index.QueryFilter("12345", new()).ShouldBeFalse();
    }

    [Fact]
    public void When_min_length_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.AddMinimumLengthQueryFilter(4);
        builder.AddQueryFilter(static (q, _) => q[0] == 'q');

        var index = builder.Build();

        index.QueryFilter("q", new()).ShouldBeFalse();
        index.QueryFilter("qq", new()).ShouldBeFalse();
        index.QueryFilter("qqq", new()).ShouldBeFalse();
        index.QueryFilter("qqqq", new()).ShouldBeTrue();
        index.QueryFilter("qqqqq", new()).ShouldBeTrue();
        index.QueryFilter("12345", new()).ShouldBeFalse();
    }

    [Fact]
    public void When_min_length_is_specifically_excluded()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.AddNoMinimumLengthQueryFilter();

        var index = builder.Build();

        index.QueryFilter("q", new()).ShouldBeTrue();
    }

    [Fact]
    public void When_fuzziness_is_func()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithFuzziness(static (q, _) => new Fuzziness(maxEditDistance: q.Text.Length, allowTransposition: true));

        var index = builder.Build();

        var queryContext = CreateQueryContext("query");
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[0], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 5, allowTransposition: true));
    }

    [Fact]
    public void When_fuzziness_is_static()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithFuzziness(4, allowTransposition: false);

        var index = builder.Build();

        var queryContext = CreateQueryContext("query");
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[0], queryContext).ShouldBe(new Fuzziness(maxEditDistance: 4, allowTransposition: false));
    }

    [Fact]
    public void When_fuzziness_is_none()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithNoFuzziness();

        var index = builder.Build();

        var queryContext = CreateQueryContext("autypo_is_awesome");
        index.FuzzinessSelector(queryContext.ExtractedQueryTokens[0], queryContext).ShouldBe(Fuzziness.None);
    }

    [Fact]
    public void When_token_ordering_is_func()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithTokenOrdering(static context => Enum.Parse<TokenOrdering>(context.Query));

        var index = builder.Build();

        index.TokenOrderingSelector(CreateQueryContext("StrictSequence")).ShouldBe(TokenOrdering.StrictSequence);
        index.TokenOrderingSelector(CreateQueryContext("InOrder")).ShouldBe(TokenOrdering.InOrder);
        index.TokenOrderingSelector(CreateQueryContext("Unordered")).ShouldBe(TokenOrdering.Unordered);
    }

    [Fact]
    public void When_token_ordering_is_InOrder()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithInOrderTokenOrdering();

        var index = builder.Build();

        var queryContext = CreateQueryContext("autypo_is_awesome");
        index.TokenOrderingSelector(queryContext).ShouldBe(TokenOrdering.InOrder);
    }

    [Fact]
    public void When_token_ordering_is_StrictSequence()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithStrictSequenceTokenOrdering();

        var index = builder.Build();

        var queryContext = CreateQueryContext("autypo_is_awesome");
        index.TokenOrderingSelector(queryContext).ShouldBe(TokenOrdering.StrictSequence);
    }

    [Fact]
    public void When_token_ordering_is_Unordered()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithUnorderedTokenOrdering();

        var index = builder.Build();

        var queryContext = CreateQueryContext("autypo_is_awesome");
        index.TokenOrderingSelector(queryContext).ShouldBe(TokenOrdering.Unordered);
    }

    [Fact]
    public void When_should_index_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithShouldIndex(static x => x[0] == 'r');

        var index = builder.Build();

        index.ShouldIndex.ShouldNotBeNull();
        index.ShouldIndex("required").ShouldBeTrue();
        index.ShouldIndex("zebra").ShouldBeFalse();
    }

    [Fact]
    public void When_partial_token_matching_is_specified_as_func()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithPartialTokenMatching(static (_, _) => PartialMatchPolicy.SomeQueryTokensRequired(0.33f));

        var index = builder.Build();

        index.CalculatePartialTokenMatchingPolicy.ShouldNotBeNull();
        index.CalculatePartialTokenMatchingPolicy([], CreateQueryContext("query")).ShouldBe(PartialMatchPolicy.SomeQueryTokensRequired(0.33f));
    }

    [Fact]
    public void When_priority_is_specified_as_func()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithPriority(_ => 20);

        var index = builder.Build();

        index.PrioritySelector(new()).ShouldBe(20);
    }

    [Fact]
    public void When_priority_is_specified_as_int()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithPriority(3);

        var index = builder.Build();

        index.PrioritySelector(new()).ShouldBe(3);
    }

    [Fact]
    public void When_candidate_filter_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        // There isn't really a way to simulate the parameters without faff,
        // so we just check that the delegate is the same reference.
        // This isn't as nice as checking the logic since this is more brittle.

        CandidateFilter<string> filter = static (_, _) => false;

        builder.WithCandidateFilter(filter);

        var index = builder.Build();

        index.CandidateFilter.ShouldBe(filter);
    }

    [Fact]
    public void When_candidate_scorer_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        // There isn't really a way to simulate the parameters without faff,
        // so we just check that the delegate is the same reference.
        // This isn't as nice as checking the logic since this is more brittle.

        CandidateScorer<string> scorer = static (_, _) => 1f;

        builder.WithCandidateScorer(scorer);

        var index = builder.Build();

        index.CandidateScorer.ShouldBe(scorer);
    }

    [Fact]
    public void When_candidate_tagger_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        // There isn't really a way to simulate the parameters without faff,
        // so we just check that the delegate is the same reference.
        // This isn't as nice as checking the logic since this is more brittle.

        CandidateTagger<string> tagger = static (_, _) => { };

        builder.WithCandidateTagger(tagger);

        var index = builder.Build();

        index.CandidateTagger.ShouldBe(tagger);
    }

    [Fact]
    public void When_analyzer_is_specified()
    {
        var builder = new AutypoIndexConfigurationBuilder<string>(static s => s);

        builder.WithTextAnalyzer(analyzer => analyzer
            .UseTokenizer(() => new TestTokenizer())
            .UseTransformer(() => new NGramTokenTransformer(2)));

        var index = builder.Build();

        index.DocumentTextAnalyzerFactory().Analyze("abc".AsMemory()).TransformedTokens.Select(t => t.Text).ShouldBe(["a b".AsMemory(), "b c".AsMemory()]);
        index.QueryTextAnalyzerFactory().Analyze("abc".AsMemory()).TransformedTokens.Select(t => t.Text).ShouldBe(["a b".AsMemory(), "b c".AsMemory()]);
    }

    private static AutypoQueryContext CreateQueryContext(string query)
    {
        return new AutypoQueryContext(
            rawQuery: query,
            query: query,
            indexName: null,
            extractedQueryTokens: query.Split(' ').Select((t, i) => new AutypoToken(i, 1, t.AsMemory(), AutypoTags.None)).ToArray(),
            transformedQueryTokens: query.Split(' ').Select((t, i) => new AutypoToken(i, 1, t.AsMemory(), AutypoTags.None)).ToArray(),
            queryTokenizedLength: query.Split(' ').Length,
            metadata: new Dictionary<string, object>(),
            documentMetadata: null!,
            indexedDocumentCount: 1,
            indexedDocumentKeysCount: 1
        );
    }

    private record TestItem(string Item1, string Item2);

    private class TestTokenizer : IAutypoTokenizer
    {
        public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
        {
            while (text.Length > 0)
            {
                var tokenSegment = new AutypoTokenSegment(leadingTrivia: 0, tokenizedLength: 1, trailingTrivia: 0);
                consumer.Accept(tokenSegment);
                tokenSegment.Slice(ref text, out _);
            }
        }
    }
}
