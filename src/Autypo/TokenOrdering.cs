namespace Autypo;

/// <summary>
/// Defines the positional constraints applied to query token matches within a document.
/// This setting influences how the relative order of matched tokens is evaluated during search.
/// </summary>
public enum TokenOrdering
{
    /// <summary>
    /// Matched query tokens must appear in the same order in the document
    /// as they do in the query. Other intervening tokens are allowed.
    /// </summary>
    InOrder = 0,

    /// <summary>
    /// Matched query tokens must appear in the same order and be adjacent
    /// (i.e., no unmatched tokens may appear between them).
    /// </summary>
    StrictSequence = 1,

    /// <summary>
    /// Matched query tokens may appear in any order within the document.
    /// The relative position of matched tokens is not enforced.
    /// </summary>
    Unordered = 2
}
