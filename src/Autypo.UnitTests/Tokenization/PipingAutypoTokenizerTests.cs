using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class PipingAutypoTokenizerTests
{

    [Fact]
    public void When_two_skip_take_transformers_are_piped()
    {
        var tokenizer = new DecoratingPipingAutypoTokenizer(
            new TestSkipTakeTransformer(1, 3),
            new TestSkipTakeTransformer(1, 2));

        var analyzer = new AutypoTextAnalyzer(tokenizer, new IdentityAutypoTokenTransformer());
        var results = analyzer.Analyze("1aAA2bBB3cCC".AsMemory());

        results.TransformedTokens.Select(r => new string(r.Text.Span)).ShouldBe(["AA", "BB", "CC"]);
    }

    [Fact]
    public void When_loop_is_required_for_piping()
    {
        var tokenizer = new DecoratingPipingAutypoTokenizer(
            new TestSkipTakeTransformer(1, 3),
            new TestOnlySpecificCharsTransformer(['a']));

        const string text = " aaa aab aba abb baa bab bba bbb axa xa";

        var analyzer = new AutypoTextAnalyzer(tokenizer, new IdentityAutypoTokenTransformer());
        var results = analyzer.Analyze(text.AsMemory()).TransformedTokens
                              .Select(r => new string(r.Text.Span))
                              .ToArray();

        results.ShouldBe(["aaa", "aa", "a", "a", "a", "aa", "a", "a", "a", "a", "a"]);
    }

    [Theory]
    [InlineData("abcdef", 1, 3, "bc", "bc")]
    [InlineData("123456", 2, 2, "345", "34")]
    [InlineData("xyz", 1, 2, "xyz", "yz")]
    [InlineData("abc", 10, 0, "abc")]
    [InlineData("aaaabbbb", 4, 4, "ab", "bbbb")]
    [InlineData("abcdefghi", 2, 2, "dfhi", "d", "h")]
    [InlineData("abcde", 1, 1, "b", "b")]
    [InlineData("foobar", 1, 2, "oa", "oo", "a")]
    [InlineData("aaabbbccc", 2, 1, "a", "a")]
    [InlineData("aaa", 1, 2, "a", "aa")]


    public void When_piping_skiptake_into_specificchar(string input, int skip, int take, string only, params string[] expected)
    {
        var tokenizer = new DecoratingPipingAutypoTokenizer(
            new TestSkipTakeTransformer(skip, take),
            new TestOnlySpecificCharsTransformer(only.ToCharArray()));

        var analyzer = new AutypoTextAnalyzer(tokenizer, new IdentityAutypoTokenTransformer());
        var results = analyzer.Analyze(input.AsMemory()).TransformedTokens
                              .Select(r => new string(r.Text.Span))
                              .ToArray();

        results.ShouldBe(expected, ignoreOrder: false);
    }

    [Theory]
    [InlineData("abcabc", "ab", 1, 2, "b", "b")]
    [InlineData("abcabc", "c", 0, 1, "c", "c")]
    [InlineData("abcdef", "xyz", 1, 2)]
    [InlineData("foobar", "oa", 0, 2, "oo", "a")]
    [InlineData("123456", "345", 1, 2, "45")]
    [InlineData("aabbaacc", "a", 2, 2)]
    [InlineData("xyzxyz", "xyz", 1, 1, "y", "x", "z")]
    [InlineData("mississippi", "s", 1, 1, "s", "s")]
    [InlineData("ababab", "ab", 1, 2, "ba", "ab")]
    [InlineData("aaaabbbbcccc", "ac", 2, 2, "aa", "cc")]


    public void When_piping_specificchar_skiptake(string input, string only, int skip, int take, params string[] expected)
    {
        var tokenizer = new DecoratingPipingAutypoTokenizer(
            new TestOnlySpecificCharsTransformer(only.ToCharArray()),
            new TestSkipTakeTransformer(skip, take));

        var analyzer = new AutypoTextAnalyzer(tokenizer, new IdentityAutypoTokenTransformer());
        var results = analyzer.Analyze(input.AsMemory()).TransformedTokens
                              .Select(r => new string(r.Text.Span))
                              .ToArray();

        results.ShouldBe(expected, ignoreOrder: false);
    }

    private class TestSkipTakeTransformer(int skip, int take) : IAutypoTokenizer
    {
        public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
        {
            while (text.Length > skip)
            {
                AutypoTokenSegment tokenSegment;

                if (text.Length < skip + take)
                {
                    tokenSegment = new AutypoTokenSegment(
                        leadingTrivia: skip,
                        tokenizedLength: text.Length - skip,
                        trailingTrivia: 0);
                }
                else
                {
                    tokenSegment = new AutypoTokenSegment(
                        leadingTrivia: skip,
                        tokenizedLength: take,
                        trailingTrivia: 0);
                }

                consumer.Accept(tokenSegment);
                tokenSegment.Slice(ref text, out var _);
            }
        }
    }

    private class TestOnlySpecificCharsTransformer(char[] only) : IAutypoTokenizer
    {
        public void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer)
        {
            while (text.Length > 0)
            {
                var span = text.Span;

                var skipLength = span.IndexOfAny(only);
                if (skipLength == -1)
                {
                    break;
                }

                AutypoTokenSegment tokenSegment;

                var tokenizedLength = span[skipLength..].IndexOfAnyExcept(only);
                if (tokenizedLength == -1)
                {
                    tokenSegment = new AutypoTokenSegment(
                        skipLength,
                        span.Length - skipLength,
                        trailingTrivia: 0);
                }
                else
                {
                    tokenSegment = new AutypoTokenSegment(
                        skipLength,
                        tokenizedLength,
                        trailingTrivia: 0);
                }

                consumer.Accept(tokenSegment);
                tokenSegment.Slice(ref text, out var _);
            }
        }
    }
}
