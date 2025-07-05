namespace Autypo;

/// <summary>
/// Provides predefined strategies for determining fuzzy matching behavior.
/// </summary>
public static class FuzzinessSelectors
{
    /// <summary>
    /// A token-length-based selector that increases allowed fuzziness for longer words:
    /// <list type="bullet">
    /// <item><description>Length ≤ 3: no edits allowed</description></item>
    /// <item><description>Length ≤ 5: one edit allowed</description></item>
    /// <item><description>Length ≤ 8: two edits allowed</description></item>
    /// <item><description>Length &gt; 8: two edits allowed with transpositions</description></item>
    /// </list>
    /// </summary>
    public static FuzzinessSelector LengthBased { get; } =
        static (token, _) => token.Text.Length switch
        {
            <= 3 => new(0, false),
            <= 5 => new(1, false),
            <= 8 => new(2, false),
            _ => new(2, true)
        };
}
