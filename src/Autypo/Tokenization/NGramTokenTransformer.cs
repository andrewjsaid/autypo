namespace Autypo.Tokenization;

/// <summary>
/// A token transformer that emits fixed-length token groups (n-grams) from the token stream.
/// </summary>
/// <remarks>
/// Tokens are accumulated and joined as they arrive. The first <c>n - 1</c> tokens are buffered
/// before the first n-gram is emitted. Each output token spans <c>n</c> consecutive inputs,
/// retaining sequence information and joining token text with a space separator.
/// 
/// For example, the tokens "quick", "brown", "fox" with n = 2 will emit:
/// "quick brown", then "brown fox".
/// </remarks>
public sealed class NGramTokenTransformer : IAutypoTokenTransformer
{
    private readonly AutypoToken?[] _tokenHistory;

    /// <summary>
    /// Initializes a new <see cref="NGramTokenTransformer"/> that produces n-gram tokens.
    /// </summary>
    /// <param name="ngramLength">The number of tokens to include in each n-gram. Must be at least 2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ngramLength"/> is less than 2.</exception>
    public NGramTokenTransformer(int ngramLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ngramLength, 2);

        _tokenHistory = new AutypoToken[ngramLength];
    }

    /// <inheritdoc />
    public void Transform(AutypoToken token, TokenConsumer consumer)
    {
        var tokenHistory = _tokenHistory;

        for (var i = 0; i < tokenHistory.Length - 1; i++)
        {
            tokenHistory[i] = tokenHistory[i + 1];
        }

        tokenHistory[^1] = token;

        if (tokenHistory[0] is not null)
        {
            consumer.Accept(AutypoToken.Concat(tokenHistory!));
        }
    }
}
