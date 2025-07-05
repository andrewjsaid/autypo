using Autypo.Tokenization;
using Shouldly;

namespace Autypo.UnitTests.Tokenization;

public class IdentityAutypoTokenizerTests
{

    [Theory]
    [InlineData("test")]
    [InlineData("two words")]
    [InlineData("three words now")]
    public void Happy_day(string query)
    {
        var tokenizer = new IdentityAutypoTokenizer();
        var analyzer = new AutypoTextAnalyzer(tokenizer, new IdentityAutypoTokenTransformer());
        
        var analysisResult = analyzer.Analyze(query.AsMemory());
        analysisResult.ExtractedTokens.Select(t => new string(t.Text.Span)).ShouldBe([query]);
    }
}
