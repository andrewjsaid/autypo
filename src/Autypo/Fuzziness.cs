using System.Diagnostics;

namespace Autypo;

/// <summary>
/// Represents the fuzzy matching configuration for a token, including maximum allowed edit distance
/// and whether transpositions are permitted.
/// </summary>
/// <remarks>
/// This structure is used to configure how tolerant the search engine is to typographical errors
/// when matching individual tokens.
/// </remarks>
[DebuggerDisplay("Distance={MaxEditDistance}, AllowTransposition={AllowTransposition}")]
public readonly struct Fuzziness(int maxEditDistance, bool allowTransposition)
{

    /// <summary>
    /// A <see cref="Fuzziness"/> instance representing exact matching with no tolerance for typos.
    /// </summary>
    public static Fuzziness None => new(0, false);

    /// <summary>
    /// The maximum edit distance allowed between the query token and indexed tokens.
    /// An edit is defined as an insertion, deletion, or substitution (or optionally, transposition).
    /// </summary>
    /// <remarks>
    /// A value of 0 requires exact match. Higher values increase match tolerance but may reduce precision.
    /// </remarks>
    public int MaxEditDistance { get; } = maxEditDistance;

    /// <summary>
    /// Indicates whether character transpositions (e.g., "chagre" → "charge") are allowed
    /// and counted as a single edit.
    /// </summary>
    public bool AllowTransposition { get; } = allowTransposition;
}
