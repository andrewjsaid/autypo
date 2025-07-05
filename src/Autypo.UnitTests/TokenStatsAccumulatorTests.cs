using Shouldly;

namespace Autypo.UnitTests;

public class TokenStatsAccumulatorTests
{
    [Fact]
    public void When_single_token_matches_once_exactly()
    {
        var accumulator = new TokenStatsAccumulator();

        accumulator.Reset();

        accumulator.ProcessHit(documentIndex: 0, keyNum: 1, matchStartBitmap: 0b01, distance: 0);

        var stats = accumulator.GetStats();

        stats.ExactStats.DocumentFrequency.ShouldBe(1);
        stats.ExactStats.DocumentKeyFrequency.ShouldBe(1);
        stats.ExactStats.CollectionFrequency.ShouldBe(1);
        stats.ExactStats.MaxTermFrequency.ShouldBe(1);
        stats.ExactStats.MinPosition.ShouldBe(0);
        stats.ExactStats.MaxPosition.ShouldBe(0);
        stats.ExactStats.AverageMinPosition.ShouldBe(0);
        stats.ExactStats.AverageMaxPosition.ShouldBe(0);

        stats.NearStats.DocumentFrequency.ShouldBe(1);
        stats.NearStats.DocumentKeyFrequency.ShouldBe(1);
        stats.NearStats.CollectionFrequency.ShouldBe(1);
        stats.NearStats.MaxTermFrequency.ShouldBe(1);
        stats.NearStats.MinPosition.ShouldBe(0);
        stats.NearStats.MaxPosition.ShouldBe(0);
        stats.NearStats.AverageMinPosition.ShouldBe(0);
        stats.NearStats.AverageMaxPosition.ShouldBe(0);

        stats.FuzzyStats.DocumentFrequency.ShouldBe(1);
        stats.FuzzyStats.DocumentKeyFrequency.ShouldBe(1);
        stats.FuzzyStats.CollectionFrequency.ShouldBe(1);
        stats.FuzzyStats.MaxTermFrequency.ShouldBe(1);
        stats.FuzzyStats.MinPosition.ShouldBe(0);
        stats.FuzzyStats.MaxPosition.ShouldBe(0);
        stats.FuzzyStats.AverageMinPosition.ShouldBe(0);
        stats.FuzzyStats.AverageMaxPosition.ShouldBe(0);
    }

    [Fact]
    public void When_single_token_matches_two_documents_with_some_keys()
    {
        var accumulator = new TokenStatsAccumulator();

        accumulator.Reset();

        accumulator.ProcessHit(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b001, distance: 0);
        accumulator.ProcessHit(documentIndex: 0, keyNum: 1, matchStartBitmap: 0b011, distance: 0);
        accumulator.ProcessHit(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b010, distance: 0);
        accumulator.ProcessHit(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b110, distance: 1);

        var stats = accumulator.GetStats();

        stats.ExactStats.DocumentFrequency.ShouldBe(2);
        stats.ExactStats.DocumentKeyFrequency.ShouldBe(3);
        stats.ExactStats.CollectionFrequency.ShouldBe(4);
        stats.ExactStats.MaxTermFrequency.ShouldBe(2);
        stats.ExactStats.MinPosition.ShouldBe(0);
        stats.ExactStats.MaxPosition.ShouldBe(1);
        stats.ExactStats.AverageMinPosition.ShouldBe(1 / 3f);
        stats.ExactStats.AverageMaxPosition.ShouldBe(2 / 3f);

        stats.NearStats.DocumentFrequency.ShouldBe(2);
        stats.NearStats.DocumentKeyFrequency.ShouldBe(3);
        stats.NearStats.CollectionFrequency.ShouldBe(5);
        stats.NearStats.MaxTermFrequency.ShouldBe(2);
        stats.NearStats.MinPosition.ShouldBe(0);
        stats.NearStats.MaxPosition.ShouldBe(2);
        stats.NearStats.AverageMinPosition.ShouldBe(1 / 3f);
        stats.NearStats.AverageMaxPosition.ShouldBe(3 / 3f);

        stats.NearStats.DocumentFrequency.ShouldBe(2);
        stats.NearStats.DocumentKeyFrequency.ShouldBe(3);
        stats.NearStats.CollectionFrequency.ShouldBe(5);
        stats.NearStats.MaxTermFrequency.ShouldBe(2);
        stats.NearStats.MinPosition.ShouldBe(0);
        stats.NearStats.MaxPosition.ShouldBe(2);
        stats.NearStats.AverageMinPosition.ShouldBe(1 / 3f);
        stats.NearStats.AverageMaxPosition.ShouldBe(3 / 3f);
    }
}
