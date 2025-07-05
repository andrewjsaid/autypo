using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class WhitespaceAutypoTokenizerTests
{

    [Theory]
    [InlineData("")]
    [InlineData("no_spaces")]
    [InlineData("word1 word2")]
    [InlineData("a aa  aaa")]
    [InlineData(" a ")]
    [InlineData(" a")]
    [InlineData("a ")]
    [InlineData(" a a ")]
    public void Whitespace_tokenizer_splits_like_String_Split(string text)
    {
        var analyzer = new AutypoTextAnalyzer(new WhitespaceAutypoTokenizer(), new IdentityAutypoTokenTransformer());

        var results = analyzer.Analyze(text.AsMemory());

        var expected = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        results.TransformedTokens.Select(r => new string(r.Text.Span)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("some text here", "some", "text", "here")]
    [InlineData("  some  text  ", "some", "text")]
    [InlineData("\tsome\ttext\t", "some", "text")]
    [InlineData("\nsome\ntext\n", "some", "text")]
    [InlineData("some\t text\nhere", "some", "text", "here")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("\t\t\t")]
    [InlineData("\n\n")]
    [InlineData("word", "word")]
    [InlineData("  word  ", "word")]
    [InlineData("word\n", "word")]
    [InlineData("hello\u00A0world", "hello", "world")]
    [InlineData("foo\u2003bar", "foo", "bar")]
    [InlineData("one\u2028two\u2029three", "one", "two", "three")]
    [InlineData("some, text.", "some,", "text.")]
    [InlineData("it's working", "it's", "working")]
    [InlineData("hello-world", "hello-world")]
    [InlineData("some\t\ntext\r\nhere", "some", "text", "here")]
    [InlineData("line1\rline2", "line1", "line2")]
    [InlineData("some     spaced     text", "some", "spaced", "text")]
    [InlineData("one\t\t\t\tword", "one", "word")]

    public void Whitespace_tokenizer_splits_correctly(string input, params string[] expected)
    {
        var analyzer = new AutypoTextAnalyzer(new WhitespaceAutypoTokenizer(), new IdentityAutypoTokenTransformer());

        var results = analyzer.Analyze(input.AsMemory());

        results.TransformedTokens.Select(r => new string(r.Text.Span)).ShouldBe(expected);
    }

}
