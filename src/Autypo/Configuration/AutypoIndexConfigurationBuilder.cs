using Autypo.Tokenization;

namespace Autypo.Configuration;

/// <summary>
/// Provides a fluent API for configuring how a single Autypo index processes, matches, and ranks documents of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The document type associated with this index.</typeparam>
public sealed class AutypoIndexConfigurationBuilder<T> where T : notnull
{
    private string? _name;
    private readonly Func<T, string?> _keySelector;
    private Func<T, IEnumerable<string>>? _additionalKeySelectors;
    private QueryExpander? _queryExpander;
    private readonly List<QueryFilter> _queryFilters = new();
    private bool? _enableCaseSensitivity;
    private TokenOrderingSelector? _tokenOrderingSelector;
    private FuzzinessSelector? _fuzzinessSelector;
    private MatchScopeSelector? _matchScopeSelector;
    private Func<T, bool>? _shouldIndex;
    private Func<AutypoTextAnalyzer>? _queryTextAnalyzerFactory;
    private Func<AutypoTextAnalyzer>? _documentTextAnalyzerFactory;
    private PartialTokenMatchingPolicySelector? _partialTokenMatchingPolicySelector;
    private CandidateScorer<T>? _candidateScorer;
    private CandidateFilter<T>? _candidateFilter;
    private IndexPrioritySelector? _prioritySelector;
    private CandidateTagger<T>? _candidateTagger;

    internal AutypoIndexConfigurationBuilder(Func<T, string?> keySelector)
    {
        _keySelector = keySelector;
    }

    /// <summary>
    /// Sets the name of this index for diagnostics and tooling.
    /// </summary>
    /// <param name="name">A descriptive label for this index. This does not need to be unique.</param>
    /// <returns>The current builder instance.</returns>
    public AutypoIndexConfigurationBuilder<T> WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Adds one or more additional strings to be indexed alongside the primary key.
    /// </summary>
    /// <param name="selector">Extracts extra keys to index for each document.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Use this to index synonyms, abbreviations, aliases, or metadata projections.
    /// These values are indexed identically to the primary key and influence matching and scoring.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithAdditionalKeys(Func<T, IEnumerable<string>> selector)
    {
        _additionalKeySelectors = selector;
        return this;
    }

    /// <summary>
    /// Configures how the raw query string is transformed into semantic variants before tokenization.
    /// </summary>
    /// <param name="expander">A delegate that rewrites the query string into one or more alternate forms.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Common use cases include stopword removal (e.g., "the office" → "office"),
    /// synonym substitution (e.g., "nyc" → "new york"), or plural normalization.
    /// 
    /// These transformations operate on the full query string, before any tokenization occurs.
    /// For token-level rewriting in performance-critical scenarios, prefer using a custom
    /// <see cref="IAutypoTokenTransformer"/> during tokenization instead.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithQueryExpander(QueryExpander expander)
    {
        _queryExpander = expander;
        return this;
    }

    /// <summary>
    /// Sets whether token matching should be case-sensitive.
    /// </summary>
    /// <param name="value">
    /// <c>true</c> to require exact casing in both query and document;
    /// <c>false</c> (default) to perform comparisons using <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
    /// </param>
    /// <returns>The current builder instance.</returns>
    public AutypoIndexConfigurationBuilder<T> WithCaseSensitivity(bool value)
    {
        _enableCaseSensitivity = value;
        return this;
    }

    /// <summary>
    /// Controls how query tokens must appear in the indexed document in terms of order and adjacency.
    /// </summary>
    /// <param name="selector">Selects a <see cref="TokenOrdering"/> policy based on the current query context.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This setting constrains the order in which tokens must match:
    /// <list type="bullet">
    ///   <item><see cref="TokenOrdering.StrictSequence"/> — tokens must appear in exact order, with no gaps.</item>
    ///   <item><see cref="TokenOrdering.InOrder"/> — tokens must appear in order, but gaps are allowed.</item>
    ///   <item><see cref="TokenOrdering.Unordered"/> — tokens may appear in any order or position.</item>
    /// </list>
    /// The default is <c>InOrder</c>, which balances precision and user flexibility.
    /// Use stricter options when structure matters (e.g., code, legal, or phrase completion).
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithTokenOrdering(TokenOrderingSelector selector)
    {
        _tokenOrderingSelector = selector;
        return this;
    }

    /// <summary>
    /// Adds a filter that determines whether the query string should be processed by this index.
    /// </summary>
    /// <param name="filter">A delegate that evaluates the query and search context.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Query filters are useful for suppressing short, empty, malformed, or blacklisted queries before tokenization.
    /// Multiple filters can be composed using <see cref="QueryFilters.All"/>.
    /// If no query filters are explicitly configured, Autypo applies a default filter that requires
    /// queries to be at least 3 characters in length.This helps avoid wasteful lookups for trivial input.
    ///
    /// As soon as any filter is manually added using <see cref = "AutypoIndexConfigurationBuilder{T}.AddQueryFilter" />,
    /// the default minimum-length behavior is disabled. In that case, it is the developer’s responsibility
    /// to enforce any necessary safeguards (e.g., minimum length or character set checks).
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> AddQueryFilter(QueryFilter filter)
    {
        _queryFilters.Add(filter);
        return this;
    }

    /// <summary>
    /// Configures fuzzy matching behavior per query token.
    /// </summary>
    /// <param name="selector">Defines the allowed edit distance and transposition behavior for each token.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// By default, Autypo uses a length-based strategy: short tokens require exact matches,
    /// while longer tokens allow greater leniency, including transpositions.
    /// Customize this to fine-tune precision/recall tradeoffs for your domain.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithFuzziness(FuzzinessSelector selector)
    {
        _fuzzinessSelector = selector;
        return this;
    }

    /// <summary>
    /// Specifies how strictly each query token must match an indexed token.
    /// </summary>
    /// <param name="selector">Selects a <see cref="MatchScope"/> for each token based on query context.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Match scope affects whether tokens require full matches or allow prefixes:
    /// <list type="bullet">
    ///   <item><see cref="MatchScope.Full"/> — token must match completely.</item>
    ///   <item><see cref="MatchScope.Prefix"/> — token may match the start of a longer token.</item>
    /// </list>
    /// The default behavior is <c>Prefix</c> for the final token (to support live typing) and <c>Full</c> for earlier tokens.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithMatchScope(MatchScopeSelector selector)
    {
        _matchScopeSelector = selector;
        return this;
    }

    /// <summary>
    /// Filters out documents that should not be indexed by this index.
    /// </summary>
    /// <param name="shouldIndex">A predicate that returns <c>true</c> for items to include.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This is useful for scoping indices to certain item types, tenants, or visibility constraints.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithShouldIndex(Func<T, bool> shouldIndex)
    {
        _shouldIndex = shouldIndex;
        return this;
    }

    /// <summary>
    /// Configures how input strings are tokenized and transformed for both queries and documents.
    /// </summary>
    /// <param name="configureBuilder">A callback that configures an <see cref="AutypoTextAnalyzerBuilder"/>.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// You can apply filters, casing, normalization, stemming, or custom token transformations.
    /// This influences both search quality and performance.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithTextAnalyzer(Action<AutypoTextAnalyzerBuilder> configureBuilder)
    {
        var builder = new AutypoTextAnalyzerBuilder();
        configureBuilder(builder);
        (_queryTextAnalyzerFactory, _documentTextAnalyzerFactory) = builder.Build();
        return this;
    }

    /// <summary>
    /// Configures partial token matching behavior, including how many tokens must match and which are required.
    /// </summary>
    /// <param name="selector">Returns a <see cref="PartialMatchPolicy"/> for the current query.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// This selector controls how many and which tokens in a query must produce a match for the document to be considered a valid result.
    /// It is evaluated per query and can adapt to token count, query intent, or domain rules.
    ///
    /// Autypo supports three primary modes of partial match policy:
    /// <list type="bullet">
    ///   <item><see cref="PartialMatchPolicy.AllQueryTokensRequired"/> — All query tokens must match. Strictest mode (100% coverage).</item>
    ///   <item><see cref="PartialMatchPolicy.SomeQueryTokensRequired"/> — At least one query token must match. Maximizes recall (0% threshold).</item>
    ///   <item>
    /// A mixed strategy where specific token indices are required (via <see cref="PartialMatchPolicy.WithRequiredQueryToken"/>),
    /// and a fractional match threshold is applied to the remaining tokens (e.g., "2 of 3 must match").
    /// </item>
    /// </list>
    ///
    /// <para>
    /// Because token indices are computed at query time, this selector can precisely control partial matching behavior
    /// in response to real query structure. For example, early tokens may be required while trailing tokens are optional.
    /// </para>
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithPartialTokenMatching(PartialTokenMatchingPolicySelector selector)
    {
        _partialTokenMatchingPolicySelector = selector;
        return this;
    }

    /// <summary>
    /// Assigns a relevance score to each matched candidate result.
    /// </summary>
    /// <param name="scorer">A delegate that computes a float score. Higher scores indicate better matches.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Use this to integrate business rank signals, popularity, or custom logic beyond lexical matching.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithCandidateScorer(CandidateScorer<T> scorer)
    {
        _candidateScorer = scorer;
        return this;
    }

    /// <summary>
    /// Filters candidates after matching, but before scoring or returning results.
    /// </summary>
    /// <param name="filter">A delegate that evaluates each candidate in query context.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Use this for post-match business rules, permission checks, or quality gates.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithCandidateFilter(CandidateFilter<T> filter)
    {
        _candidateFilter = filter;
        return this;
    }

    /// <summary>
    /// Attaches tags or metadata to matched candidates for enrichment or diagnostics.
    /// </summary>
    /// <param name="tagger">A delegate that annotates candidates during result post-processing.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Tags may be used for UI decoration, source tracking, or downstream filtering.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithCandidateTagger(CandidateTagger<T> tagger)
    {
        _candidateTagger = tagger;
        return this;
    }

    /// <summary>
    /// Sets the relative priority of this index when searching across multiple indices.
    /// </summary>
    /// <param name="selector">A delegate that returns a numeric priority. Higher values are favored.</param>
    /// <returns>The current builder instance.</returns>
    /// <remarks>
    /// Autypo evaluates higher-priority indices first. If matches are found, lower-priority indices are skipped.
    /// This short-circuiting can reduce latency and improve result quality for intent-specific indices.
    /// </remarks>
    public AutypoIndexConfigurationBuilder<T> WithPriority(IndexPrioritySelector selector)
    {
        _prioritySelector = selector;
        return this;
    }

    /// <summary>
    /// Finalizes the builder and creates the index configuration.
    /// </summary>
    /// <returns>The completed <see cref="AutypoIndexConfiguration{T}"/>.</returns>
    /// <remarks>
    /// Intended for internal use by the engine. Callers should use higher-level configuration APIs.
    /// </remarks>
    internal AutypoIndexConfiguration<T> Build()
    {
        return new AutypoIndexConfiguration<T>
        {
            Name = _name,
            KeySelector = _keySelector,
            AdditionalKeySelectors = _additionalKeySelectors,
            QueryExpander = _queryExpander ?? (static (s, _) => [s]),
            QueryFilter = _queryFilters.Count switch
            {
                0 => QueryFilters.MinimumLength(3),
                1 => _queryFilters[0],
                _ => QueryFilters.All(_queryFilters.ToArray())
            },
            EnableCaseSensitivity = _enableCaseSensitivity ?? false,
            TokenOrderingSelector = _tokenOrderingSelector ?? (static _ => TokenOrdering.InOrder),
            FuzzinessSelector = _fuzzinessSelector ?? FuzzinessSelectors.LengthBased,
            MatchScopeSelector = _matchScopeSelector ?? MatchScopeSelectors.PrefixFinalTokenOnly,
            ShouldIndex = _shouldIndex,
            QueryTextAnalyzerFactory = _queryTextAnalyzerFactory ?? AutypoTextAnalyzer.CreateDefaultTextAnalyzerFactory(),
            DocumentTextAnalyzerFactory = _documentTextAnalyzerFactory ?? AutypoTextAnalyzer.CreateDefaultTextAnalyzerFactory(),
            CalculatePartialTokenMatchingPolicy = _partialTokenMatchingPolicySelector,
            CandidateScorer = _candidateScorer ?? CandidateScorers.DefaultScorer,
            CandidateFilter = _candidateFilter,
            CandidateTagger = _candidateTagger,
            PrioritySelector = _prioritySelector ?? (static _ => 0)
        };
    }
}
