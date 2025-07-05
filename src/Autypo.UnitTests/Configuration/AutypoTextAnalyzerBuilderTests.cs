using Autypo.Configuration;
using Autypo.Tokenization;
using Shouldly;
namespace Autypo.UnitTests.Configuration;

public class AutypoTextAnalyzerBuilderTests
{

    [Fact]
    public void When_only_tokenizer_is_set_then_transformer_has_identity_default()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseTokenizer(() => new WhitespaceAutypoTokenizer());

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("hello world".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello", "world"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello", "world"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("hello world".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello", "world"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello", "world"]);
    }

    [Fact]
    public void When_only_transformer_is_set_then_tokenizer_has_whitespace_default()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseTransformer(() => new NGramTokenTransformer(ngramLength: 2));

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("hello, world!".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello,", "world!"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello, world!"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("hello, world!".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello,", "world!"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["hello, world!"]);
    }

    [Fact]
    public void When_both_tokenizer_and_transformer_are_set()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseTokenizer(() => new NCharsTokenizer(ngramLength: 2));
        builder.UseTransformer(() => new NGramTokenTransformer(ngramLength: 2));

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12", "34", "56"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12 34", "34 56"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12", "34", "56"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12 34", "34 56"]);
    }

    [Fact]
    public void When_only_query_transformers_are_set()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseQueryTokenizer(() => new NCharsTokenizer(ngramLength: 2));
        builder.UseQueryTransformer(() => new NGramTokenTransformer(ngramLength: 2));

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12", "34", "56"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12 34", "34 56"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["123456"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["123456"]);
    }

    [Fact]
    public void When_only_document_transformers_are_set()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseDocumentTokenizer(() => new NCharsTokenizer(ngramLength: 2));
        builder.UseDocumentTransformer(() => new NGramTokenTransformer(ngramLength: 2));

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["123456"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["123456"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("123456".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12", "34", "56"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["12 34", "34 56"]);
    }

    [Fact]
    public void When_using_additive_transformer()
    {
        var builder = new AutypoTextAnalyzerBuilder();

        builder.UseTransformer(() => new NGramTokenTransformer(ngramLength: 2));
        builder.UseAlsoQueryTransformer(() => new NGramTokenTransformer(ngramLength: 2));
        builder.UseAlsoDocumentTransformer(() => new NGramTokenTransformer(ngramLength: 3));

        var analyzer = builder.Build();

        var analysisResult = analyzer.QueryTextAnalyzer().Analyze("1 2 3 4 5 6".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["1", "2", "3", "4", "5", "6"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["1 2", "2 3", "1 2 2 3", "3 4", "2 3 3 4", "4 5", "3 4 4 5", "5 6", "4 5 5 6"]);

        analysisResult = analyzer.DocumentTextAnalyzer().Analyze("1 2 3 4 5 6".AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["1", "2", "3", "4", "5", "6"]);
        analysisResult.TransformedTokens.Select(t => new string(t.Text.Span)).ShouldBe(["1 2", "2 3", "3 4", "1 2 2 3 3 4", "4 5", "2 3 3 4 4 5", "5 6", "3 4 4 5 5 6"]);
    }

    private class NCharsTokenizer(int ngramLength) : IAutypoTokenizer
    {
        public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
        {
            while (text.Length >= ngramLength)
            {
                var tokenSegment = new AutypoTokenSegment(leadingTrivia: 0, tokenizedLength: ngramLength, trailingTrivia: 0);
                consumer.Accept(tokenSegment);
                tokenSegment.Slice(ref text, out _);
            }
        }
    }
}
