using Autypo.Configuration;
using Autypo.Tokenization;
using Shouldly;

namespace Autypo.IntegrationTests;

/// <summary>
/// Tests which check the various toggles for adjusting recall.
/// </summary>
public class RecallIntegrationTests
{

    [Fact]
    public async Task When_default()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaaa",
                "aaaaaa bbbb",
                "aaaaaa bbbb ccc"
            ])
            .WithUnlimitedResults());

        // ===== Unrelated =====
        completer.Complete("zzzzz").Count().ShouldBe(0);

        // ===== Exact matches =====
        completer.Complete("aaaaaa").Count().ShouldBe(3);
        completer.Complete("bbbb").Count().ShouldBe(2);
        completer.Complete("ccc").Count().ShouldBe(1);

        // ===== Near matches =====
        completer.Complete("aaaaax").Count().ShouldBe(3);
        completer.Complete("bbbx").Count().ShouldBe(2);
        completer.Complete("ccx").Count().ShouldBe(0, "Too short to fuzz");

        // ===== Fuzzy matches =====
        completer.Complete("aaaaxx").Count().ShouldBe(3);
        completer.Complete("bbxx").Count().ShouldBe(0, "Too short for 2 errors");
        completer.Complete("ccx").Count().ShouldBe(0, "Too short to fuzz");

        // ===== Ordering =====
        completer.Complete("aaaaaa bbbb ccc").Count().ShouldBe(1, "Sequential");
        completer.Complete("aaaaaa bbbb").Count().ShouldBe(2, "Sequential");
        completer.Complete("bbbb ccc").Count().ShouldBe(1, "Sequential");
        completer.Complete("aaaaaa ccc").Count().ShouldBe(1, "In Order");
        completer.Complete("ccc aaaaaa bbbb").Count().ShouldBe(0, "Unordered");
        completer.Complete("ccc bbbb").Count().ShouldBe(0, "Unordered");

        // ===== Case =====
        completer.Complete("AAAaaa").Count().ShouldBe(3);

        // ===== Prefix =====
        completer.Complete("aaaaa").Count().ShouldBe(3);
        completer.Complete("aaaaa bb").Count().ShouldBe(2);
        completer.Complete("aaaaa b").Count().ShouldBe(2);
        completer.Complete("aaaaa c").Count().ShouldBe(1);
        completer.Complete("c aaaaa").Count().ShouldBe(0, "Only final token is prefixed");

        // ===== Partial Token Matching =====
        completer.Complete("aaaaaa").Count().ShouldBe(3);
        completer.Complete("aaaaaa dddd").Count().ShouldBe(0);

        // ===== Minimum Query Length =====
        completer.Complete("aaa").Count().ShouldBe(3);
        completer.Complete("aa").Count().ShouldBe(0);

        // ===== Compound =====
        completer.Complete("aaa aaa").Count().ShouldBe(0, "Compound matching is opt-in");
        completer.Complete("aaaaaabbbb").Count().ShouldBe(0, "Compound matching is opt-in");
    }

    [Fact]
    public async Task When_no_fuzz()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaa bbbbb",
                "aaaaa ccccc ",
                "aaaaa bbbbb ccccc",
                "aaaaa ccccc aaaaa ddddd",
                "bbbbb ccccc ddddd"
            ])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithUnorderedTokenOrdering())
            .WithUnlimitedResults());

        // ===== Unrelated =====
        completer.Complete("zzzzz").Count().ShouldBe(0);

        // ===== Exact matches =====
        completer.Complete("aaaaa").Count().ShouldBe(4);
        completer.Complete("bbbbb").Count().ShouldBe(3);
        completer.Complete("ccccc").Count().ShouldBe(4);
        completer.Complete("ddddd").Count().ShouldBe(2);

        // ===== Near matches =====
        completer.Complete("aaaax").Count().ShouldBe(0);
        completer.Complete("bbbbx").Count().ShouldBe(0);
        completer.Complete("ccccx").Count().ShouldBe(0);
        completer.Complete("ddddx").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_unordered()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaa bbbbb",
                "aaaaa ccccc ",
                "aaaaa bbbbb ccccc",
                "aaaaa ccccc aaaaa ddddd",
                "bbbbb ccccc ddddd"
            ])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithUnorderedTokenOrdering())
            .WithUnlimitedResults());

        // ===== Unrelated =====
        completer.Complete("zzzzz").Count().ShouldBe(0);

        // ===== Exact matches =====
        completer.Complete("aaaaa").Count().ShouldBe(4);
        completer.Complete("bbbbb").Count().ShouldBe(3);
        completer.Complete("ccccc").Count().ShouldBe(4);
        completer.Complete("ddddd").Count().ShouldBe(2);

        // ===== Ordering =====
        completer.Complete("aaaaa bbbbb").Count().ShouldBe(2);
        completer.Complete("bbbbb aaaaa").Count().ShouldBe(2);

        completer.Complete("aaaaa ccccc").Count().ShouldBe(3);
        completer.Complete("ccccc aaaaa").Count().ShouldBe(3);

        completer.Complete("ccccc bbbbb").Count().ShouldBe(2);
        completer.Complete("bbbbb ccccc").Count().ShouldBe(2);

        completer.Complete("aaaaa ddddd").Count().ShouldBe(1);
        completer.Complete("ddddd aaaaa").Count().ShouldBe(1);

        completer.Complete("aaaaa ccccc ddddd").Count().ShouldBe(1);
        completer.Complete("aaaaa ddddd ccccc").Count().ShouldBe(1);
        completer.Complete("ccccc aaaaa ddddd").Count().ShouldBe(1);
        completer.Complete("ccccc ddddd aaaaa").Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_strict_sequence()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaa bbbbb",
                "aaaaa ccccc ",
                "aaaaa bbbbb ccccc",
                "aaaaa ccccc aaaaa ddddd",
                "bbbbb ccccc ddddd"
            ])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithStrictSequenceTokenOrdering())
            .WithUnlimitedResults());

        // ===== Unrelated =====
        completer.Complete("zzzzz").Count().ShouldBe(0);

        // ===== Exact matches =====
        completer.Complete("aaaaa").Count().ShouldBe(4);
        completer.Complete("bbbbb").Count().ShouldBe(3);
        completer.Complete("ccccc").Count().ShouldBe(4);
        completer.Complete("ddddd").Count().ShouldBe(2);

        // ===== Ordering =====
        completer.Complete("aaaaa bbbbb").Count().ShouldBe(2);
        completer.Complete("bbbbb aaaaa").Count().ShouldBe(0);

        completer.Complete("aaaaa ccccc").Count().ShouldBe(2);
        completer.Complete("ccccc aaaaa").Count().ShouldBe(1);

        completer.Complete("ccccc bbbbb").Count().ShouldBe(0);
        completer.Complete("bbbbb ccccc").Count().ShouldBe(2);

        completer.Complete("aaaaa ddddd").Count().ShouldBe(1);
        completer.Complete("ddddd aaaaa").Count().ShouldBe(0);

        completer.Complete("aaaaa ccccc ddddd").Count().ShouldBe(0);
        completer.Complete("aaaaa ddddd ccccc").Count().ShouldBe(0);
        completer.Complete("ccccc aaaaa ddddd").Count().ShouldBe(1);
        completer.Complete("ccccc ddddd aaaaa").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_case_sensitive()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaa bbbbb",
                "aaaaa",
                "bbbbb"
            ])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithCaseSensitivity(true))
            .WithUnlimitedResults());

        // ===== Unrelated =====
        completer.Complete("zzzzz").Count().ShouldBe(0);

        // ===== Exact matches =====
        completer.Complete("aaaaa").Count().ShouldBe(2);
        completer.Complete("Aaaaa").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_prefix_matching_is_optional()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource([
                "aaaaa bbbbb",
                "aaaaa",
                "bbbbb"
            ])
            .WithIndex(s => s, index => index
                .WithNoFuzziness()
                .WithMatchScope(static (t, c) => t.SequenceStart == c.QueryTokenizedLength - 1 && (bool)c.Metadata["prefix"] ? MatchScope.Prefix : MatchScope.Full))
            .WithUnlimitedResults());

        completer.Complete("aaaaa bb", new AutypoSearchContext { Metadata = new Dictionary<string, object> { { "prefix", false } } }).Count().ShouldBe(0);
        completer.Complete("aaaaa bb", new AutypoSearchContext { Metadata = new Dictionary<string, object> { { "prefix", true } } }).Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_minimum_query_length_is_2()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithIndex(s => s, index => index.AddMinimumLengthQueryFilter(2))
            .WithUnlimitedResults());

        completer.Complete("a").Count().ShouldBe(0);
        completer.Complete("aa").Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_query_filter_is_exact()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithIndex(s => s, index => index.AddQueryFilter((q, _) => q.Length == 5))
            .WithUnlimitedResults());

        completer.Complete("aaaa").Count().ShouldBe(0);
        completer.Complete("aaaaa").Count().ShouldBe(1);
        completer.Complete("aaaaaa").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_any_token_can_match()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa"])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithUnorderedTokenOrdering().WithPartialTokenMatching())
            .WithUnlimitedResults());

        completer.Complete("aaaaa").Count().ShouldBe(1);

        completer.Complete("aaaaa bbbbb").Count().ShouldBe(1);
        completer.Complete("bbbbb aaaaa").Count().ShouldBe(1);
    }

    [Fact]
    public async Task When_half_tokens_can_match()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa bbbbb"])
            .WithIndex(s => s, index => index.WithNoFuzziness().WithUnorderedTokenOrdering().WithPartialTokenMatching(0.5f))
            .WithUnlimitedResults());

        completer.Complete("aaaaa bbbbb").Count().ShouldBe(1);

        completer.Complete("aaaaa bbbbb ccccc").Count().ShouldBe(1);
        completer.Complete("aaaaa bbbbb ccccc ddddd").Count().ShouldBe(1);
        completer.Complete("aaaaa bbbbb ccccc ddddd eeeee").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_first_token_must_match()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa bbbbb"])
            .WithIndex(s => s, index => index
                .WithNoFuzziness()
                .WithUnorderedTokenOrdering()
                .WithPartialTokenMatching(static (_, _) => PartialMatchPolicy.SomeQueryTokensRequired().WithRequiredQueryToken(0)))
            .WithUnlimitedResults());

        completer.Complete("aaaaa bbbbb").Count().ShouldBe(1);

        completer.Complete("aaaaa bbbbb ccccc").Count().ShouldBe(1);
        completer.Complete("ccccc aaaaa bbbbb").Count().ShouldBe(0);
    }

    [Fact]
    public async Task When_compound_tokens_are_enabled()
    {
        var completer = await AutypoFactory.CreateCompleteAsync(config => config
            .WithDataSource(["aaaaa bbbbb"])
            .WithIndex(s => s, index => index
                .WithFuzziness(2) // We need 2 fuzziness to account for missing spaces
                .WithFinalTokenFullMatchScope() // Don't match by mistake - we want compound matches after all
                .WithTextAnalyzer(analyzer => analyzer
                    .UseTokenizer(() => new WhitespaceAutypoTokenizer())
                    .UseAlsoTransformer(() => new NGramTokenTransformer(ngramLength: 2)))
                )
            .WithUnlimitedResults());

        completer.Complete("aa").Count().ShouldBe(0);
        completer.Complete("aa aa").Count().ShouldBe(1);
        completer.Complete("aaaaa bbbbb").Count().ShouldBe(1);
        completer.Complete("aaaaabbbbb").Count().ShouldBe(1);
        completer.Complete("aaaaabbb bb").Count().ShouldBe(1);

        completer.Complete("bbbbbaaaaa").Count().ShouldBe(0);
    }

}