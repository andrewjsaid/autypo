using Shouldly;

namespace Autypo.UnitTests;

public class IndexPrioritizerTests
{
    [Fact]
    public void When_single_index_then_it_is_always_first_and_only()
    {
        var prioritizer = new IndexPrioritizer<int>([new(0, 100)]);

        prioritizer.GetNext(false, out int readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(0);

        prioritizer.GetNext(false, out _).ShouldBeFalse();
    }

    [Fact]
    public void When_two_indices_of_same_priority_then_both_run()
    {
        var prioritizer = new IndexPrioritizer<int>([
            new(0, 100),
            new(1, 100),
        ]);

        prioritizer.GetNext(false, out int readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(0);

        prioritizer.GetNext(true, out readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(1);

        prioritizer.GetNext(true, out _).ShouldBeFalse();
    }

    [Fact]
    public void When_two_indices_of_different_priority_then_only_first_run()
    {
        var prioritizer = new IndexPrioritizer<int>([
            new(0, 50),
            new(1, 100),
        ]);

        prioritizer.GetNext(false, out int readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(0);

        prioritizer.GetNext(true, out readerIndex).ShouldBeFalse();
    }

    [Fact]
    public void When_two_indices_of_different_priority_and_first_has_no_matches_then_second_run()
    {
        var prioritizer = new IndexPrioritizer<int>([
            new(0, 50),
            new(1, 100),
        ]);

        prioritizer.GetNext(false, out int readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(0);

        prioritizer.GetNext(false, out readerIndex).ShouldBeTrue();
        readerIndex.ShouldBe(1);

        prioritizer.GetNext(false, out _).ShouldBeFalse();
    }
}
