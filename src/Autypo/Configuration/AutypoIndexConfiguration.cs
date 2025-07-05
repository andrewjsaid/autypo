using Autypo.Tokenization;

namespace Autypo.Configuration;

/// <summary>
/// Represents shared indexing configuration for an Autypo index, including tokenization, 
/// transformation, and matching behavior.
/// </summary>
internal abstract class AutypoIndexConfiguration
{
    /// <summary>
    /// Gets the name of this index, used for diagnostics and tooling purposes.
    /// </summary>
    /// <remarks>
    /// This value does not need to be unique. It may appear in logs, error messages, or developer tools.
    /// </remarks>
    public required string? Name { get; init; }


    /// <summary>
    /// Gets a value indicating whether token matching should be case-sensitive.
    /// </summary>
    /// <remarks>
    /// If <c>true</c>, casing is respected during both indexing and query matching. 
    /// If <c>false</c>, comparisons use case-insensitive rules based on <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
    /// </remarks>
    public required bool EnableCaseSensitivity { get; init; }

    /// <summary>
    /// Gets the delegate used to expand the original query into multiple transformed variants before tokenization.
    /// </summary>
    /// <remarks>
    /// This is the first stage of query preprocessing. It operates on the entire raw query string and can be used to:
    /// <list type="bullet">
    ///   <item>Remove or rewrite stopwords (e.g., "the united states" → "united states")</item>
    ///   <item>Substitute synonyms or canonical forms (e.g., "nyc" → "new york")</item>
    ///   <item>Generate multiple semantic variants of a user query</item>
    /// </list>
    /// 
    /// The returned sequence represents logically distinct interpretations of the query. Each will be tokenized and evaluated independently.
    /// 
    /// For performance-critical synonym or token-level substitutions, prefer using a custom
    /// <see cref="IAutypoTokenTransformer"/> during tokenization instead.
    /// </remarks>
    public required QueryExpander QueryExpander { get; init; }

    /// <summary>
    /// Gets the delegate used to filter queries based on content or context.
    /// </summary>
    /// <remarks>
    /// This can be used to block certain queries (e.g., reserved words or diagnostics).
    /// For performance-critical filtering specific terms (e.g., stopwords), prefer using a custom
    /// <see cref="IAutypoTokenTransformer"/> during tokenization instead.
    /// </remarks>
    public required QueryFilter QueryFilter { get; init; }

    /// <summary>
    /// Gets the delegate that determines the token ordering constraints to apply during query matching.
    /// </summary>
    /// <remarks>
    /// This selector controls how strictly the order of query tokens must match the order of document tokens. 
    /// The selected <see cref="TokenOrdering"/> affects which matches are considered valid:
    /// <list type="bullet">
    ///   <item><see cref="TokenOrdering.StrictSequence"/> requires query tokens to appear contiguously and in order.</item>
    ///   <item><see cref="TokenOrdering.InOrder"/> requires the token order to match, but allows gaps.</item>
    ///   <item><see cref="TokenOrdering.Unordered"/> allows tokens to appear in any position.</item>
    /// </list>
    /// Use stricter ordering constraints when positional semantics matter (e.g., code completion, legal terms),
    /// and relax them for more flexible search experiences.
    /// </remarks>
    public required TokenOrderingSelector TokenOrderingSelector { get; init; }

    /// <summary>
    /// Gets the delegate that controls fuzzy matching behavior for each query token.
    /// </summary>
    /// <remarks>
    /// This selector determines the allowed edit distance and whether transpositions are permitted when matching query tokens.
    /// 
    /// <para>
    /// <b>Default behavior:</b> Autypo uses a length-based strategy, where longer tokens allow more leniency:
    /// shorter tokens require exact or near-exact matches, while longer tokens can tolerate more edits,
    /// including transpositions if sufficiently long.
    /// </para>
    ///
    /// <para>
    /// Customize this selector to fine-tune match tolerance, improve error correction, or enforce stricter precision for high-risk fields.
    /// </para>
    /// </remarks>
    public required FuzzinessSelector FuzzinessSelector { get; init; }

    /// <summary>
    /// Gets the delegate that determines the match scope for each query token.
    /// </summary>
    /// <remarks>
    /// The match scope defines how strictly a query token must align with an indexed token:
    /// <list type="bullet">
    ///   <item><see cref="MatchScope.Full"/> — the token must match an indexed token completely (fuzzy or exact).</item>
    ///   <item><see cref="MatchScope.Prefix"/> — the token may match the beginning of an indexed token.</item>
    /// </list>
    /// This is primarily used to support autocomplete behaviors, where in-progress typing should yield matches.
    /// 
    /// <para>
    /// <b>Default behavior:</b> Autypo uses <c>Prefix</c> for the final token in the query and <c>Full</c> for all earlier tokens.
    /// This strikes a balance between recall and precision for live input scenarios.
    /// </para>
    /// </remarks>
    public required MatchScopeSelector MatchScopeSelector { get; init; }

    /// <summary>
    /// Gets the factory that produces the analyzer used to tokenize query input.
    /// </summary>
    public required Func<AutypoTextAnalyzer> QueryTextAnalyzerFactory { get; init; }

    /// <summary>
    /// Gets the factory that produces the analyzer used to tokenize document input during indexing.
    /// </summary>
    public required Func<AutypoTextAnalyzer> DocumentTextAnalyzerFactory { get; init; }

    /// <summary>
    /// Gets the delegate that determines which query tokens must match and how strictly partial matches are allowed.
    /// </summary>
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
    public required PartialTokenMatchingPolicySelector? CalculatePartialTokenMatchingPolicy { get; init; }

    /// <summary>
    /// Gets the delegate used to assign a relative priority to this index during search.
    /// </summary>
    /// <remarks>
    /// Higher priority values are favored. When merging across indices, Autypo will 
    /// evaluate higher-priority indices first. If results are found, lower-priority indices are skipped.
    /// This can reduce latency by short-circuiting evaluation. 
    /// For example, you might prefer to search ISO country codes before country names.
    /// </remarks>
    public required IndexPrioritySelector PrioritySelector { get; init; }

}

/// <summary>
/// Represents a strongly-typed index configuration for documents of type <typeparamref name="T"/>.
/// Defines how data is projected, tokenized, and evaluated for this index.
/// </summary>
/// <typeparam name="T">The document type associated with this index.</typeparam>
internal sealed class AutypoIndexConfiguration<T> : AutypoIndexConfiguration
{
    /// <summary>
    /// Gets the delegate used to extract the primary string value to index from each item.
    /// </summary>
    /// <remarks>
    /// This string is the core projection used for tokenization and matching. 
    /// Items with <c>null</c> or empty key values will be excluded from the index.
    /// </remarks>
    public required Func<T, string?> KeySelector { get; init; }

    /// <summary>
    /// Gets the delegate used to extract additional strings to index from each item.
    /// </summary>
    /// <remarks>
    /// These supplementary values are tokenized and included in the index alongside the primary key.
    /// Useful for indexing aliases, abbreviations, alternate representations, etc.
    /// </remarks>
    public required Func<T, IEnumerable<string>>? AdditionalKeySelectors { get; init; }

    /// <summary>
    /// Gets an optional predicate that determines whether the item should be included in this index.
    /// </summary>
    /// <remarks>
    /// If the predicate returns <c>false</c>, the item is excluded from this index (but may appear in others).
    /// </remarks>
    public required Func<T, bool>? ShouldIndex { get; init; }

    /// <summary>
    /// Gets the component responsible for assigning a relevance score to candidate results.
    /// </summary>
    /// <remarks>
    /// Scores may reflect lexical similarity, semantic rank, domain-specific logic, or any other metric.
    /// Higher scores indicate higher relevance and improve final ranking.
    /// </remarks>
    public required CandidateScorer<T> CandidateScorer { get; init; }

    /// <summary>
    /// Gets an optional component used to filter candidate results after matching but before scoring.
    /// </summary>
    /// <remarks>
    /// This is typically used to apply business rules, permissions, or quality thresholds.
    /// </remarks>
    public required CandidateFilter<T>? CandidateFilter { get; init; }

    /// <summary>
    /// Gets an optional component that can annotate matched candidates with metadata.
    /// </summary>
    /// <remarks>
    /// Taggers can be used to enrich results with contextual metadata such as
    /// source field, match type, or domain-specific tags.
    /// </remarks>
    public required CandidateTagger<T>? CandidateTagger { get; init; }
}