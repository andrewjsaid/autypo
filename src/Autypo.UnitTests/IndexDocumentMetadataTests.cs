using Shouldly;

namespace Autypo.UnitTests;

public class IndexDocumentMetadataTests
{
    [Fact]
    public void When_get_all_metadata()
    {
        IndexDocumentMetadata[] docMeta = [
            new IndexDocumentMetadata(0, 1), // first doc 1 key
            new IndexDocumentMetadata(1, 2), // second doc 2 keys
            new IndexDocumentMetadata(3, 3) // third doc 3 keys
        ];

        IndexKeyDocumentMetadata[] keyMeta = [
            ..CreateKeyMeta("a"),
            ..CreateKeyMeta("b", "bb"),
            ..CreateKeyMeta("c", "cc", "ccc")
        ];

        var d0Meta = docMeta[0].GetMetadata(keyMeta);
        d0Meta.Length.ShouldBe(1);
        d0Meta[0].KeyNum.ShouldBe(0);
        d0Meta[0].TokenizedLength.ShouldBe(1);

        var d1Meta = docMeta[1].GetMetadata(keyMeta);
        d1Meta.Length.ShouldBe(2);
        d1Meta[0].KeyNum.ShouldBe(0);
        d1Meta[0].TokenizedLength.ShouldBe(1);
        d1Meta[1].KeyNum.ShouldBe(1);
        d1Meta[1].TokenizedLength.ShouldBe(2);

        var d2Meta = docMeta[2].GetMetadata(keyMeta);
        d2Meta.Length.ShouldBe(3);
        d2Meta[0].KeyNum.ShouldBe(0);
        d2Meta[0].TokenizedLength.ShouldBe(1);
        d2Meta[1].KeyNum.ShouldBe(1);
        d2Meta[1].TokenizedLength.ShouldBe(2);
        d2Meta[2].KeyNum.ShouldBe(2);
        d2Meta[2].TokenizedLength.ShouldBe(3);
    }
    [Fact]
    public void When_get_specific_metadata()
    {
        IndexDocumentMetadata[] docMeta = [
            new IndexDocumentMetadata(0, 1), // first doc 1 key
            new IndexDocumentMetadata(1, 2), // second doc 2 keys
            new IndexDocumentMetadata(3, 3) // third doc 3 keys
        ];

        IndexKeyDocumentMetadata[] keyMeta = [
            ..CreateKeyMeta("a"),
            ..CreateKeyMeta("b", "bb"),
            ..CreateKeyMeta("c", "cc", "ccc")
        ];

        Assert.Throws<InvalidOperationException>(() => docMeta[0].GetMetadata(keyMeta, keyNum: 1));
        Assert.Throws<InvalidOperationException>(() => docMeta[1].GetMetadata(keyMeta, keyNum: 2));
        Assert.Throws<InvalidOperationException>(() => docMeta[2].GetMetadata(keyMeta, keyNum: 3));

        var d0K0Meta = docMeta[0].GetMetadata(keyMeta, 0);
        d0K0Meta.KeyNum.ShouldBe(0);
        d0K0Meta.TokenizedLength.ShouldBe(1);

        var d1K0Meta = docMeta[1].GetMetadata(keyMeta, 0);
        d1K0Meta.KeyNum.ShouldBe(0);
        d1K0Meta.TokenizedLength.ShouldBe(1);

        var d1K1Meta = docMeta[1].GetMetadata(keyMeta, 1);
        d1K1Meta.KeyNum.ShouldBe(1);
        d1K1Meta.TokenizedLength.ShouldBe(2);

        var d2K0Meta = docMeta[2].GetMetadata(keyMeta, 0);
        d2K0Meta.KeyNum.ShouldBe(0);
        d2K0Meta.TokenizedLength.ShouldBe(1);

        var d2K1Meta = docMeta[2].GetMetadata(keyMeta, 1);
        d2K1Meta.KeyNum.ShouldBe(1);
        d2K1Meta.TokenizedLength.ShouldBe(2);

        var d2K2Meta = docMeta[2].GetMetadata(keyMeta, 2);
        d2K2Meta.KeyNum.ShouldBe(2);
        d2K2Meta.TokenizedLength.ShouldBe(3);
    }

    private static IEnumerable<IndexKeyDocumentMetadata> CreateKeyMeta(params string[] keys) => keys.Select((k, i) => new IndexKeyDocumentMetadata(i, 0ul, k.Length, null, null));
}
