using System.Runtime.InteropServices;

namespace Autypo;

/// <summary>
/// A match ranker that retains all matches and returns them sorted by score descending.
/// </summary>
internal sealed class UnboundedMatchRanker : IMatchRanker
{
    private readonly Dictionary<int, RankedDocument> _scores = new();

    /// <inheritdoc/>
    public void Process(int documentIndex, float score, AutypoTags tags)
    {
        ref var rankedDocument = ref CollectionsMarshal.GetValueRefOrAddDefault(_scores, documentIndex, out var exists);
        if (!exists || rankedDocument.Score < score)
        {
            rankedDocument = new RankedDocument(documentIndex, score, tags);
        }
    }

    /// <inheritdoc/>
    public int Count => _scores.Count;

    /// <inheritdoc/>
    public IEnumerable<RankedDocument> GetRankedDocuments()
        => _scores.Values.OrderByDescending(d => d.Score);
}
