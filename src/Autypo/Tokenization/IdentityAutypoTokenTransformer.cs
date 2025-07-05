namespace Autypo.Tokenization;

/// <summary>
/// A no-op transformer that emits the input token as-is without modification.
/// </summary>
/// <remarks>
/// Useful as the default or placeholder transformer in pipelines that do not require rewriting tokens.
/// </remarks>
public sealed class IdentityAutypoTokenTransformer : IAutypoTokenTransformer
{
    /// <summary>
    /// A shared singleton instance.
    /// </summary>
    public static IdentityAutypoTokenTransformer Instance { get; } = new();

    /// <inheritdoc />
    public void Transform(AutypoToken token, TokenConsumer consumer)
    {
        consumer.Accept(token);
    }
}
