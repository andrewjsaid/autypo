namespace Autypo.Tokenization;

/// <summary>
/// Provides a layered transformation pipeline that delegates to an inner transformer and allows
/// further modification or decoration of each intermediate result.
/// </summary>
/// <remarks>
/// This base class abstracts the pattern of applying an inner transformation step followed by
/// additional logic per transformed token via <see cref="TransformCore"/>. It also supports emitting
/// the results of the inner transformer into the output stream directly.
/// </remarks>
/// <param name="inner">The inner transformer whose output will be further processed.</param>
/// <param name="emitInner">
/// If <c>true</c>, tokens produced by the inner transformer will be emitted to the final output
/// before being passed to <see cref="TransformCore"/> for further processing.
/// </param>
internal abstract class PipingAutypoTokenTransformer(IAutypoTokenTransformer inner, bool emitInner) : IAutypoTokenTransformer
{
    private readonly TokenConsumer _innerConsumer = new();

    /// <summary>
    /// Applies a two-stage transformation to the input token. First, delegates to the inner transformer;
    /// then applies <see cref="TransformCore"/> to each of the inner results.
    /// </summary>
    /// <param name="token">The token to be transformed.</param>
    /// <param name="consumer">The token consumer that receives the final transformed tokens.</param>
    public void Transform(AutypoToken token, TokenConsumer consumer)
    {
        _innerConsumer.Reset();

        inner.Transform(token, _innerConsumer);

        foreach (var transformedToken in _innerConsumer.Tokens)
        {
            if (emitInner)
            {
                // Optionally emit original token before passing it to next transformer
                consumer.Accept(transformedToken);
            }

            TransformCore(transformedToken, consumer);
        }
    }

    /// <summary>
    /// Performs the outer transformation step on each token produced by the inner transformer.
    /// </summary>
    /// <param name="token">A token emitted by the inner transformer.</param>
    /// <param name="consumer">The consumer that receives the final output tokens.</param>
    protected abstract void TransformCore(AutypoToken token, TokenConsumer consumer);
}