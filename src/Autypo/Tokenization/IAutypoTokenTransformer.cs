namespace Autypo.Tokenization;

/// <summary>
/// Transforms an input token into zero or more output tokens, allowing for normalization,
/// augmentation, or deletion of token data during preprocessing.
/// </summary>
/// <remarks>
/// Used as part of the token analysis pipeline to reshape or enrich token streams
/// before indexing or querying.
/// </remarks>
public interface IAutypoTokenTransformer
{
    /// <summary>
    /// Applies a transformation to the specified token and emits the result
    /// (zero or more tokens) through the given <paramref name="consumer"/>.
    /// </summary>
    /// <param name="token">The input token to transform.</param>
    /// <param name="consumer">The target consumer that receives transformed tokens.</param>
    void Transform(AutypoToken token, TokenConsumer consumer);
}

/// <summary>
/// Represents a collector for tokens produced during transformation.
/// Used by <see cref="IAutypoTokenTransformer"/> implementations to emit output tokens.
/// </summary>
public sealed class TokenConsumer
{
    private readonly List<AutypoToken> _tokens = [];

    /// <summary>
    /// Accepts a transformed token into the consumer.
    /// </summary>
    /// <param name="token">The token to accept.</param>
    public void Accept(AutypoToken token) => _tokens.Add(token);

    /// <summary>
    /// Clears the internal token buffer.
    /// Typically called before each transformation to reset state.
    /// </summary>
    public void Reset() => _tokens.Clear();

    /// <summary>
    /// Gets the collection of tokens that have been accepted.
    /// </summary>
    public IReadOnlyList<AutypoToken> Tokens => _tokens;
}