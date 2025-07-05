namespace Autypo.Tokenization;

/// <summary>
/// A specialized token transformer that delegates the second transformation stage to a separate
/// outer transformer, after applying the inner transformation stage.
/// </summary>
/// <remarks>
/// This class composes two <see cref="IAutypoTokenTransformer"/> instances: an inner transformer
/// for primary transformation, and an outer transformer for additional decoration or augmentation.
/// The <paramref name="emitInner"/> flag controls whether the results of the inner transformer
/// should be included in the output alongside the outer transformation.
/// </remarks>
/// <param name="inner">The inner transformer that produces intermediate tokens.</param>
/// <param name="outer">The outer transformer that further transforms each token produced by the inner transformer.</param>
/// <param name="emitInner">
/// If <c>true</c>, each token produced by the inner transformer is emitted before being transformed by the outer.
/// </param>
internal sealed class DecoratingPipingAutypoTokenTransformer(
    IAutypoTokenTransformer inner,
    IAutypoTokenTransformer outer,
    bool emitInner)
    : PipingAutypoTokenTransformer(inner, emitInner)
{
    /// <summary>
    /// Delegates transformation of the token to the outer transformer.
    /// </summary>
    /// <param name="token">The token to be transformed by the outer transformer.</param>
    /// <param name="consumer">The consumer that receives the final transformed tokens.</param>
    protected override void TransformCore(AutypoToken token, TokenConsumer consumer)
    {
        outer.Transform(token, consumer);
    }
}