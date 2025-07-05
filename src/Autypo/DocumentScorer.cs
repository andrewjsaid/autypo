namespace Autypo;

/// <summary>
/// Represents a method that assigns a base score to a document,
/// independent of the user's query.
/// </summary>
/// <typeparam name="T">
/// The type of the document being indexed.
/// </typeparam>
/// <param name="document">
/// The document being considered. This is the original value stored in the autocomplete system,
/// prior to any query analysis or token matching.
/// </param>
/// <returns>
/// A floating-point score representing the intrinsic relevance or likelihood of the document.
/// This value should be normalized to a suitable range (e.g. [0,1] or log-scaled),
/// and will be combined with match-based scores to produce the final ranking.
/// 
/// Common uses include boosting based on:
/// - Popularity (e.g. search frequency)
/// - Domain importance (e.g. featured products)
/// - Recency (e.g. recent searches or records)
/// </returns>
public delegate float DocumentScorer<in T>(T document);
