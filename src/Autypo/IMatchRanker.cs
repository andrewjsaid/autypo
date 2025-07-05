namespace Autypo;

/// <summary>
/// Defines the contract for collecting and ranking matched documents during search.
/// </summary>
/// <remarks>
/// An <see cref="IMatchRanker"/> receives matches through <see cref="Process"/>,
/// retains only the best-scoring result for each document (based on <see cref="RankedDocument.DocumentIndex"/>),
/// and returns the final set of top-ranked results via <see cref="GetRankedDocuments"/>.
/// </remarks>
internal interface IMatchRanker
{
    /// <summary>
    /// Adds or evaluates a new match for the specified document.
    /// If a higher score for the same document index is already present, it is retained instead.
    /// </summary>
    /// <param name="documentIndex">The index of the matched document.</param>
    /// <param name="score">The score to assign to this match.</param>
    /// <param name="tags">Optional metadata associated with the match.</param>
    void Process(int documentIndex, float score, AutypoTags tags);
    
    /// <summary>
    /// Gets the number of distinct matched documents currently tracked.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Retrieves the final ranked matches, ordered by descending score.
    /// Only the best match per <see cref="RankedDocument.DocumentIndex"/> is returned.
    /// </summary>
    /// <returns>A sequence of <see cref="RankedDocument"/>s representing the top results.</returns>
    IEnumerable<RankedDocument> GetRankedDocuments();
}
