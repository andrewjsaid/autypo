namespace Autypo;

/// <summary>
/// Contains request-specific metadata available during query preparation and dispatch.
/// </summary>
/// <remarks>
/// <see cref="AutypoSearchContext"/> is passed to selectors and uninitialized data handlers
/// before tokenization occurs. It may contain user-defined values such as authentication info,
/// input origin, or UI context. This allows flexible control of configuration decisions at query time.
/// </remarks>
public sealed class AutypoSearchContext
{
    /// <summary>
    /// Arbitrary key/value pairs used to influence query-time behavior,
    /// such as scoring, filtering, or tokenizer selection.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; set; }
}
