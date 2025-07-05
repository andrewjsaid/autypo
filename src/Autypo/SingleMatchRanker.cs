namespace Autypo;

/// <summary>
/// A match ranker that retains only the single best-scoring match.
/// </summary>
internal sealed class SingleMatchRanker : IMatchRanker
{
    private bool _hasValue;
    private RankedDocument _best;

    /// <inheritdoc/>
    public void Process(int documentIndex, float score, AutypoTags tags)
    {
        if (!_hasValue || score > _best.Score)
        {
            _best = new RankedDocument(documentIndex, score, tags);
            _hasValue = true;
        }
    }

    /// <inheritdoc/>
    public int Count => _hasValue ? 1 : 0;

    /// <inheritdoc/>
    public IEnumerable<RankedDocument> GetRankedDocuments()
    {
        return [_best];
    }
}
