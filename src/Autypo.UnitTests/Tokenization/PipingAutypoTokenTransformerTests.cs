using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class PipingAutypoTokenTransformerTests
{

    [Theory]
    [InlineData("test", "test")]
    [InlineData("foo-bar", "foo-bar")]
    [InlineData("123", "123")]
    [InlineData("multi word", "multi word")]
    [InlineData("Ünicode", "Ünicode")]
    [InlineData(" spaced ", " spaced ")]
    [InlineData("!!!", "!!!")]
    [InlineData("t-e-s-t", "t-e-s-t")]
    [InlineData("Σ-test", "Σ-test")]
    [InlineData("", "")]

    public void When_piping_identity_into_identity(string input, params string[] expected)
    {
        RunTest(
            inner: new TestIdentityTransformer(),
            outer: new TestIdentityTransformer(),
            input,
            expected);
    }

    [Theory]
    [InlineData("abc", "x-abc")]
    [InlineData("test", "x-test")]
    [InlineData("123", "x-123")]
    [InlineData("foo bar", "x-foo bar")]
    [InlineData("Ünicode", "x-Ünicode")]
    [InlineData(" spaced ", "x- spaced ")]
    [InlineData("!!!", "x-!!!")]
    [InlineData("hyphen-word", "x-hyphen-word")]
    [InlineData("Σigma", "x-Σigma")]
    [InlineData("", "x-")]

    public void When_piping_prefix_into_identity(string input, params string[] expected)
    {
        RunTest(
            inner: new TestPrefixTransformer("x-"),
            outer: new TestIdentityTransformer(),
            input,
            expected);
    }

    [Theory]
    [InlineData("abc", "x-abc")]
    [InlineData("test", "x-test")]
    [InlineData("123", "x-123")]
    [InlineData("foo bar", "x-foo bar")]
    [InlineData("Ünicode", "x-Ünicode")]
    [InlineData(" spaced ", "x- spaced ")]
    [InlineData("!!!", "x-!!!")]
    [InlineData("hyphen-word", "x-hyphen-word")]
    [InlineData("Σigma", "x-Σigma")]
    [InlineData("", "x-")]

    public void When_piping_identity_into_prefix(string input, params string[] expected)
    {
        RunTest(
            inner: new TestIdentityTransformer(),
            outer: new TestPrefixTransformer("x-"),
            input,
            expected);
    }

    [Theory]
    [InlineData("foo-bar", "foo", "bar")]
    [InlineData("a-b-c", "a", "b", "c")]
    [InlineData("-a-b-", "a", "b")]
    [InlineData("one--two", "one", "two")]
    [InlineData("no-split", "no", "split")]
    [InlineData("--")]
    [InlineData("")]
    [InlineData("123-456", "123", "456")]
    [InlineData("a", "a")]
    [InlineData("foo-bar-baz", "foo", "bar", "baz")]

    public void When_piping_split_into_identity(string input, params string[] expected)
    {
        RunTest(
            inner: new TestSplitterTransformer("-"),
            outer: new TestIdentityTransformer(),
            input,
            expected);
    }

    [Theory]
    [InlineData("foo-bar", ">foo", ">bar")]
    [InlineData("a-b-c", ">a", ">b", ">c")]
    [InlineData("-a-b-", ">a", ">b")]
    [InlineData("one--two", ">one", ">two")]
    [InlineData("split-me-now", ">split", ">me", ">now")]
    [InlineData("--")]
    [InlineData("")]
    [InlineData("1-2-3", ">1", ">2", ">3")]
    [InlineData("end-dash-", ">end", ">dash")]
    [InlineData("unicode-Ünicode", ">unicode", ">Ünicode")]

    public void When_piping_split_into_prefix(string input, params string[] expected)
    {
        RunTest(
            inner: new TestSplitterTransformer("-"),
            outer: new TestPrefixTransformer(">"),
            input,
            expected);
    }

    [Theory]
    [InlineData("foo-bar")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("!!!")]
    [InlineData("123")]
    [InlineData("one two")]
    [InlineData("hyphen-word")]
    [InlineData("Ünicode")]
    [InlineData(" spaced ")]
    [InlineData("drop-me")]

    public void When_piping_identity_into_dropall(string input, params string[] expected)
    {
        RunTest(
            inner: new TestIdentityTransformer(),
            outer: new TestDropAllTransformer(),
            input,
            expected);
    }

    [Theory]
    [InlineData("foo-bar")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("!!!")]
    [InlineData("123")]
    [InlineData("one two")]
    [InlineData("hyphen-word")]
    [InlineData("Ünicode")]
    [InlineData(" spaced ")]
    [InlineData("drop-me")]

    public void When_piping_dropall_into_identity(string input, params string[] expected)
    {
        RunTest(
            inner: new TestDropAllTransformer(),
            outer: new TestIdentityTransformer(),
            input,
            expected);
    }

    private void RunTest(
        IAutypoTokenTransformer inner,
        IAutypoTokenTransformer outer,
        string input,
        string[] expected)
    {
        var token = new AutypoToken(0, 1, input.AsMemory(), tags: AutypoTags.None);

        var transformer = new DecoratingPipingAutypoTokenTransformer(inner, outer, emitInner: false);

        var consumer = new TokenConsumer();

        transformer.Transform(token, consumer);

        var output = consumer.Tokens.Select(t => t.Text.ToString()).ToArray();

        output.ShouldBe(expected, ignoreOrder: false);
    }


    private class TestIdentityTransformer : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer) => consumer.Accept(token);
    }

    private class TestPrefixTransformer(string prefix) : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer)
        {
            var newText = prefix + token.Text.ToString();
            consumer.Accept(token.WithText(newText.AsMemory()));
        }
    }

    private class TestSplitterTransformer(string delimiter) : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer)
        {
            var parts = token.Text.ToString().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                consumer.Accept(token.WithText(part.AsMemory()));
            }
        }
    }

    private class TestDropAllTransformer : IAutypoTokenTransformer
    {
        public void Transform(AutypoToken token, TokenConsumer consumer) { }
    }
}
