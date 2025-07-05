using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class NGramTokenTransformerTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("a b", "a b")]
    [InlineData("a b c", "a b", "b c")]
    [InlineData("a b c d", "a b", "b c", "c d")]
    public void When_bigram(string input, params string[] expected)
    {
        var analyzer = new AutypoTextAnalyzer(new WhitespaceAutypoTokenizer(), new NGramTokenTransformer(2));

        var results = analyzer.Analyze(input.AsMemory());

        results.TransformedTokens.Select(r => new string(r.Text.Span)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a b")]
    [InlineData("a b c", "a b c")]
    [InlineData("a b c d", "a b c", "b c d")]
    public void When_trigram(string input, params string[] expected)
    {
        var analyzer = new AutypoTextAnalyzer(new WhitespaceAutypoTokenizer(), new NGramTokenTransformer(3));

        var results = analyzer.Analyze(input.AsMemory());

        results.TransformedTokens.Select(r => new string(r.Text.Span)).ShouldBe(expected);
    }
}
