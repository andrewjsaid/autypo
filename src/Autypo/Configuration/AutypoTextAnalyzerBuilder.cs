using Autypo.Tokenization;

namespace Autypo.Configuration;

/// <summary>
/// Provides a fluent API for configuring tokenization and transformation behavior
/// for both query and document analysis.
/// </summary>
/// <remarks>
/// This builder controls how raw strings are tokenized and normalized into token streams
/// for indexing and query execution.
///
/// Custom tokenizers and transformers can be configured independently for queries and documents,
/// or shared across both for symmetry. This enables advanced pipelines such as:
/// <list type="bullet">
///   <item>Splitting text using custom word boundaries</item>
///   <item>Applying stemming, lowercasing, or synonym expansion</item>
///   <item>Stacking multiple transformers for layered processing</item>
/// </list>
/// If no components are registered, a default whitespace tokenizer and identity transformer are used.
/// </remarks>
public sealed class AutypoTextAnalyzerBuilder
{
    private readonly List<Func<IAutypoTokenizer>> _queryTokenizers = new();
    private readonly List<Func<IAutypoTokenTransformer>> _queryTransformers = new();

    private readonly List<Func<IAutypoTokenizer>> _documentTokenizers = new();
    private readonly List<Func<IAutypoTokenTransformer>> _documentTransformers = new();

    internal AutypoTextAnalyzerBuilder()
    {

    }

    /// <summary>
    /// Configures a shared tokenizer to be used for both query and document analysis.
    /// </summary>
    /// <param name="tokenizer">
    /// A factory that produces a new <see cref="IAutypoTokenizer"/> instance for each analysis run.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoTextAnalyzerBuilder UseTokenizer(Func<IAutypoTokenizer> tokenizer)
    {
        _queryTokenizers.Add(tokenizer);
        _documentTokenizers.Add(tokenizer);
        return this;
    }

    /// <summary>
    /// Configures a tokenizer exclusively for query analysis.
    /// </summary>
    /// <param name="tokenizer">A factory for <see cref="IAutypoTokenizer"/> used during queries.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoTextAnalyzerBuilder UseQueryTokenizer(Func<IAutypoTokenizer> tokenizer)
    {
        _queryTokenizers.Add(tokenizer);
        return this;
    }

    /// <summary>
    /// Configures a tokenizer exclusively for document indexing.
    /// </summary>
    /// <param name="tokenizer">A factory for <see cref="IAutypoTokenizer"/> used during indexing.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoTextAnalyzerBuilder UseDocumentTokenizer(Func<IAutypoTokenizer> tokenizer)
    {
        _documentTokenizers.Add(tokenizer);
        return this;
    }

    /// <summary>
    /// Configures a shared token transformer for both query and document processing.
    /// </summary>
    /// <param name="transformer">
    /// A factory that produces an <see cref="IAutypoTokenTransformer"/>. Each transformer replaces
    /// the previous token stream.
    /// </param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Transformers configured via <c>Use*</c> are considered exclusive: they own the transformation
    /// pipeline and replace prior ones.
    /// </remarks>
    public AutypoTextAnalyzerBuilder UseTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        _queryTransformers.Add(transformer);
        _documentTransformers.Add(transformer);
        return this;
    }

    /// <summary>
    /// Configures a token transformer used only during query analysis.
    /// </summary>
    /// <param name="transformer">A factory for a query-specific token transformer.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoTextAnalyzerBuilder UseQueryTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        _queryTransformers.Add(transformer);
        return this;
    }

    /// <summary>
    /// Configures a token transformer used only during document indexing.
    /// </summary>
    /// <param name="transformer">A factory for a document-specific token transformer.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoTextAnalyzerBuilder UseDocumentTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        _documentTransformers.Add(transformer);
        return this;
    }

    /// <summary>
    /// Adds a transformer to the token transformation pipeline for both query and document analysis.
    /// Unlike <see cref="UseTransformer"/>, which semantically "owns" the pipeline,
    /// <c>UseAlsoTransformer</c> simply appends a transformer to the chain without overriding prior ones.
    /// This allows for composing multiple transformation steps that are applied in sequence.
    /// </summary>
    /// <param name="transformer">A factory that creates a new <see cref="IAutypoTokenTransformer"/> instance.</param>
    /// <returns>The current <see cref="AutypoTextAnalyzerBuilder"/> for fluent chaining.</returns>
    /// <remarks>
    /// All transformers in the pipeline receive the output of the previous transformer.
    /// This includes those added via <c>UseAlsoTransformer</c> or <c>UseTransformer</c>.
    /// 
    /// <para>Example pipeline:</para>
    /// <code>
    /// .UseTransformer(A)
    /// .UseAlsoTransformer(B)
    /// .UseTransformer(C)
    /// </code>
    /// Transformer B receives output from A, and Transformer C receives output from both A and B.
    /// </remarks>
    public AutypoTextAnalyzerBuilder UseAlsoTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        var factory = () => new DecoratingPipingAutypoTokenTransformer(IdentityAutypoTokenTransformer.Instance, transformer(), emitInner: true);
        _documentTransformers.Add(factory);
        _queryTransformers.Add(factory);
        return this;
    }

    /// <summary>
    /// Adds a transformer to the token transformation pipeline for query analysis.
    /// Unlike <see cref="UseTransformer"/>, which semantically "owns" the pipeline,
    /// <c>UseAlsoTransformer</c> simply appends a transformer to the chain without overriding prior ones.
    /// This allows for composing multiple transformation steps that are applied in sequence.
    /// </summary>
    /// <param name="transformer">A factory that creates a new <see cref="IAutypoTokenTransformer"/> instance.</param>
    /// <returns>The current <see cref="AutypoTextAnalyzerBuilder"/> for fluent chaining.</returns>
    /// <remarks>
    /// All transformers in the pipeline receive the output of the previous transformer.
    /// This includes those added via <c>UseAlsoTransformer</c> or <c>UseTransformer</c>.
    /// 
    /// <para>Example pipeline:</para>
    /// <code>
    /// .UseTransformer(A)
    /// .UseAlsoTransformer(B)
    /// .UseTransformer(C)
    /// </code>
    /// Transformer B receives output from A, and Transformer C receives output from both A and B.
    /// </remarks>
    public AutypoTextAnalyzerBuilder UseAlsoQueryTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        var factory = () => new DecoratingPipingAutypoTokenTransformer(IdentityAutypoTokenTransformer.Instance, transformer(), emitInner: true);
        _queryTransformers.Add(factory);
        return this;
    }

    /// <summary>
    /// Adds a transformer to the token transformation pipeline for document analysis.
    /// Unlike <see cref="UseTransformer"/>, which semantically "owns" the pipeline,
    /// <c>UseAlsoTransformer</c> simply appends a transformer to the chain without overriding prior ones.
    /// This allows for composing multiple transformation steps that are applied in sequence.
    /// </summary>
    /// <param name="transformer">A factory that creates a new <see cref="IAutypoTokenTransformer"/> instance.</param>
    /// <returns>The current <see cref="AutypoTextAnalyzerBuilder"/> for fluent chaining.</returns>
    /// <remarks>
    /// All transformers in the pipeline receive the output of the previous transformer.
    /// This includes those added via <c>UseAlsoTransformer</c> or <c>UseTransformer</c>.
    /// 
    /// <para>Example pipeline:</para>
    /// <code>
    /// .UseTransformer(A)
    /// .UseAlsoTransformer(B)
    /// .UseTransformer(C)
    /// </code>
    /// Transformer B receives output from A, and Transformer C receives output from both A and B.
    /// </remarks>
    public AutypoTextAnalyzerBuilder UseAlsoDocumentTransformer(Func<IAutypoTokenTransformer> transformer)
    {
        var factory = () => new DecoratingPipingAutypoTokenTransformer(IdentityAutypoTokenTransformer.Instance, transformer(), emitInner: true);
        _documentTransformers.Add(factory);
        return this;
    }

    internal (Func<AutypoTextAnalyzer> QueryTextAnalyzer, Func<AutypoTextAnalyzer> DocumentTextAnalyzer) Build()
    {
        var queryAnalyzer = Build(_queryTokenizers, _queryTransformers);
        var documentAnalyzer = Build(_documentTokenizers, _documentTransformers);
        return (queryAnalyzer, documentAnalyzer);
    }

    private static Func<AutypoTextAnalyzer> Build(List<Func<IAutypoTokenizer>> tokenizers, List<Func<IAutypoTokenTransformer>> transformers)
    {
        var tokenizerPipelineFactory = tokenizers.Count == 0 
            ? static () => new WhitespaceAutypoTokenizer()
            : tokenizers.Aggregate(static (a, b) => () => new DecoratingPipingAutypoTokenizer(a(), b()));

        var transformerPipelineFactory = transformers.Count == 0
            ? static () => IdentityAutypoTokenTransformer.Instance
            : transformers.Aggregate(static (a, b) => () => new DecoratingPipingAutypoTokenTransformer(a(), b(), emitInner: false));

        return () => new AutypoTextAnalyzer(tokenizerPipelineFactory(), transformerPipelineFactory());
    }
}
