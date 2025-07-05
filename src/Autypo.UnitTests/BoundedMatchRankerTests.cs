using Shouldly;

namespace Autypo.UnitTests;

public class BoundedMatchRankerTests
{

    [Fact]
    public void When_has_single_result_then_it_is_returned_with_tags()
    {
        var ranker = new BoundedMatchRanker(maxResults: 10);

        var tags = AutypoTags.None;
        tags.Set("hello", "world");

        ranker.Process(23, 3.14f, tags);

        var documents = ranker.GetRankedDocuments().ToArray();

        documents.ShouldHaveSingleItem();
        documents[0].DocumentIndex.ShouldBe(23);
        documents[0].Score.ShouldBe(3.14f);
        documents[0].Tags.TryGetValue("hello", out string? tag).ShouldBeTrue();
        tag.ShouldBe("world");
    }


    [Fact]
    public void When_result_has_better_score_it_is_kept()
    {
        var ranker = new BoundedMatchRanker(maxResults: 10);

        var tags1 = AutypoTags.None;
        tags1.Set("best", false);

        ranker.Process(23, 3.14f, tags1);


        var tags2 = AutypoTags.None;
        tags2.Set("best", true);

        ranker.Process(23, 9.99f, tags2);

        var documents = ranker.GetRankedDocuments().ToArray();

        documents.ShouldHaveSingleItem();
        documents[0].DocumentIndex.ShouldBe(23);
        documents[0].Score.ShouldBe(9.99f);
        documents[0].Tags.TryGetValue("best", out bool isBest).ShouldBeTrue();
        isBest.ShouldBeTrue();
    }

    [Fact]
    public void When_has_3_different_results()
    {
        var ranker = new BoundedMatchRanker(maxResults: 3);

        ranker.Process(11, 2f, AutypoTags.None);
        ranker.Process(22, 1f, AutypoTags.None);
        ranker.Process(33, 3f, AutypoTags.None);
        ranker.Process(44, 0f, AutypoTags.None);
        ranker.Process(55, 5f, AutypoTags.None);

        var documents = ranker.GetRankedDocuments().ToArray();

        documents.Length.ShouldBe(3);
        documents[0].DocumentIndex.ShouldBe(55);
        documents[1].DocumentIndex.ShouldBe(33);
        documents[2].DocumentIndex.ShouldBe(11);
    }

    [Fact]
    public void When_has_3_different_results_with_duplicates()
    {
        var ranker = new BoundedMatchRanker(maxResults: 3);

        ranker.Process(11, 2f, CreateTags(1));
        ranker.Process(44, 0f, CreateTags(2));
        ranker.Process(44, 5f, CreateTags(3));
        ranker.Process(22, 1f, CreateTags(4));
        ranker.Process(44, 6f, CreateTags(5));
        ranker.Process(44, 5.5f, CreateTags(6));
        ranker.Process(33, 3f, CreateTags(7));

        var documents = ranker.GetRankedDocuments().ToArray();

        documents.Length.ShouldBe(3);
        documents[0].DocumentIndex.ShouldBe(44);
        AssertTags(documents[0].Tags, 5);
        documents[1].DocumentIndex.ShouldBe(33);
        documents[2].DocumentIndex.ShouldBe(11);
    }


    private AutypoTags CreateTags(int value)
    {
        var result = new AutypoTags();
        result.Set("value", value);
        return result;
    }

    private void AssertTags(AutypoTags tags, int expected)
    {
        tags.TryGetValue("value", out var value).ShouldBeTrue();
        value.ShouldBe(expected);
    }
}
