using Shouldly;

namespace Autypo.UnitTests;

public class AutypoTagsTests
{
    [Fact]
    public void When_tag_has_single_value()
    {
        var tags = new AutypoTags();

        tags.TryGetValue("hello", out string? value).ShouldBeFalse();
        value.ShouldBeNull();

        tags.Remove("hello").ShouldBeFalse();

        tags.Set("hello", "world");

        tags.TryGetValue("hello", out value).ShouldBeTrue();
        value.ShouldBe("world");

        tags.Remove("hello").ShouldBeTrue();

        tags.Remove("hello").ShouldBeFalse();

        tags.TryGetValue("hello", out value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void When_tag_has_multiple_value()
    {
        var tags = new AutypoTags();

        tags.TryGetValue("hello1", out string? stringValue).ShouldBeFalse();
        stringValue.ShouldBeNull();

        tags.Set("hello1", "world1");

        tags.TryGetValue("hello2", out int intValue).ShouldBeFalse();
        intValue.ShouldBe(0);

        tags.Set("hello2", 2);

        tags.TryGetValue("hello1", out stringValue).ShouldBeTrue();
        stringValue.ShouldBe("world1");

        tags.TryGetValue("hello2", out intValue).ShouldBeTrue();
        intValue.ShouldBe(2);

        tags.Remove("hello1").ShouldBeTrue();

        tags.TryGetValue("hello1", out stringValue).ShouldBeFalse();
        stringValue.ShouldBeNull();

        tags.TryGetValue("hello2", out intValue).ShouldBeTrue();
        intValue.ShouldBe(2);
    }

    [Fact]
    public void When_copying_from_other_tags()
    {
        var tags = new AutypoTags();

        var tagsFrom = new AutypoTags();
        tagsFrom.Set("1", 1);
        tagsFrom.Set("2", 2);

        tags.TryGetValue("1", out int _).ShouldBeFalse();

        tags.CopyFrom(tagsFrom);

        tags.TryGetValue("1", out int v1).ShouldBeTrue();
        v1.ShouldBe(1);

        tags.TryGetValue("2", out int v2).ShouldBeTrue();
        v2.ShouldBe(2);

        tags.Remove("1").ShouldBeTrue();

        tagsFrom = new AutypoTags();
        tagsFrom.Set("2", 2.1m);
        tagsFrom.Set("3", 3);
        tagsFrom.Set("4", 4);

        tags.CopyFrom(tagsFrom);

        tags.TryGetValue("1", out int _).ShouldBeFalse();

        tags.TryGetValue("2", out decimal v21).ShouldBeTrue();
        v21.ShouldBe(2.1m);

        tags.TryGetValue("3", out int v3).ShouldBeTrue();
        v3.ShouldBe(3);

        tags.TryGetValue("4", out int v4).ShouldBeTrue();
        v4.ShouldBe(4);
    }

    [Fact]
    public void When_clearing()
    {
        var tags = new AutypoTags();

        tags.Set("1", 1);
        tags.Set("2", 2);

        tags.Clear();

        tags.Values.Length.ShouldBe(0);

        tags.Set("1", 1);
        tags.Set("2", 2);
        tags.Set("1", 1);
        tags.Set("2", 2);
        tags.Set("3", 1);

        tags.Values.Length.ShouldBe(3);

        tags.TryGetValue("2", out var value2).ShouldBeTrue();
        value2.ShouldBe(2);

        tags.Clear();

        tags.Values.Length.ShouldBe(0);
    }
}
