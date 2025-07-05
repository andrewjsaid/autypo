using Autypo.Configuration;
using Shouldly;

namespace Autypo.IntegrationTests;

/// <summary>
/// Tests which check the that ranking is still behaving.
/// </summary>
public class RankingIntegrationTests
{

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task Objectively_better_scenarios_as_expected(string query, string[] sorted)
    {
        var attemptsToAvoidRandomSuccess = 10;

        var items = (string[])sorted.Clone();
        for (int i = 0; i < attemptsToAvoidRandomSuccess; i++)
        {
            Random.Shared.Shuffle(items);

            var results = await SortAsync(query, items);
            results.ShouldBe(sorted);
        }
    }

    private static async Task<string[]> SortAsync(string query, IEnumerable<string> items)
    {
        var completer = await AutypoFactory.CreateCompleteAsync(c => c
                            .WithDataSource(items)
                            .WithUnlimitedResults()
                            .WithIndex(i => i, i => i
                                .WithUnorderedTokenOrdering()
                                .WithPartialTokenMatching()
                            ));

        var results = completer.Complete(query);

        return results.ToArray();
    }

    public static TheoryData<string, string[]> Scenarios()
    {
        var data = new TheoryData<string, string[]>();

        data.Add("complete", ["complete", "complet", "complt"]);

        data.Add("hello", ["hello", "hello hello", "hello hello hello"]);

        data.Add("hello hello hello", ["hello hello hello", "hello hello", "hello"]);

        data.Add("hello world", ["hello world", "hello world world", "my hello world", "hello my world", "world hello", "world of hello", "hello"]);

        data.Add("hello world", ["hello world", "hello world today", "world hello"]);

        data.Add("hello world", ["hello world", "world hello", "world", "hallo word", "some world"]);

        data.Add("ini", ["init", "initial", "initiate", "inimitable"]);

        data.Add("hello my world", ["my hello world", "hallo my world", "hello world", "some very long hello which my oh my is as long as the world"]);
        
        data.Add("artificial intelligence", ["artificial intelligence", "artificial intelligence in the 21st century", "intelligence artificial", "artificial", "intelligence"]);

        data.Add("chocolate cake", ["chocolate cake", "chocolate cake recipe", "chocolate", "cake"]);

        data.Add("rocket launch", ["rocket launch", "rocket launc", "roket launch"]);

        data.Add("sun moon stars", ["sun moon stars", "sun stars moon", "sun moon", "moon stars"]);

        data.Add("green apple", ["green apple", "green apples", "gren apple", "green", "apple"]);

        data.Add("happy path", ["happy path", "happy path case", "this is a happy path", "happy", "path"]);

        data.Add("true story", ["true story", "true story story", "story true", "true"]);

        data.Add("night sky", ["night sky", "night sky stars", "the night sky is beautiful"]);

        data.Add("ice cream", ["ice cream", "ice creem", "ice", "cream"]);

        return data;
    }
}
