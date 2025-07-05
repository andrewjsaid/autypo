using System.Diagnostics;
using Autypo.Configuration;
using Levenshtypo;

namespace Autypo;

/// <summary>
/// Represents a searchable in-memory index for a single <typeparamref name="T"/> document type.
/// </summary>
/// <typeparam name="T">The type of documents stored in the index.</typeparam>
/// <remarks>
/// IndexReader is responsible for evaluating queries against a prebuilt trie of indexed tokens,
/// returning match evidence and associated metadata used for scoring and filtering.
/// </remarks>
internal sealed class IndexReader<T>(
    AutypoIndexConfiguration<T> indexConfiguration,
    LevenshtrieSet<TokenHit> trie,
    DocumentMetadata<T>[] documentMetadata,
    IndexDocumentMetadata[] indexDocumentMetadata,
    IndexKeyDocumentMetadata[] indexKeyDocumentMetadata,
    int indexedDocumentCount,
    int indexedDocumentKeysCount)
{
    private const int MaxTokensPerDocument = 64;

    /// <summary>
    /// Searches the index using the provided query string and search context.
    /// </summary>
    /// <param name="query">The input query string submitted by the user.</param>
    /// <param name="searchContext">Contextual information used to influence search behavior and ranking.</param>
    /// <returns>
    /// A collection of <see cref="IndexReaderMatches"/> representing match results for
    /// the expanded and filtered versions of the query.
    /// </returns>
    public IEnumerable<IndexReaderMatches> Search(string query, AutypoSearchContext searchContext)
    {
        var results = new List<IndexReaderMatches>();

        foreach (var expanded in indexConfiguration.QueryExpander(query, searchContext))
        {
            if (indexConfiguration.QueryFilter(expanded, searchContext))
            {
                var matches = SearchInternal(query, expanded, searchContext);
                if (matches is not null)
                {
                    results.Add(matches);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Retrieves the index’s priority for the current search context.
    /// </summary>
    /// <param name="searchContext">The current search context.</param>
    /// <returns>An integer priority value used to rank this index among others.</returns>
    public int GetPriority(AutypoSearchContext searchContext)
    {
        return indexConfiguration.PrioritySelector(searchContext);
    }

    /// <summary>
    /// Performs the core search logic against this index using a normalized and optionally expanded query string.
    /// </summary>
    /// <param name="rawQuery">The original user-submitted query string.</param>
    /// <param name="query">The normalized or expanded query to execute.</param>
    /// <param name="searchContext">The current search context with user metadata.</param>
    /// <returns>
    /// An <see cref="IndexReaderMatches"/> object if matches are found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs tokenization, fuzzy matching, and partial match evaluation
    /// according to the configured analyzers and match policies.
    /// </remarks>
    private IndexReaderMatches? SearchInternal(string rawQuery, string query, AutypoSearchContext searchContext)
    {
        // First we tokenize the query
        var textAnalyzer = indexConfiguration.QueryTextAnalyzerFactory();
        var analysisResult = textAnalyzer.Analyze(query.AsMemory());
        var tokens = analysisResult.TransformedTokens;
        var tokenizedLength = analysisResult.TokenizedLength;

        if (tokens.Length == 0)
        {
            // Importantly this is different from empty string handling,
            // as each index may have different tokenization it will
            // lead to unexpected results.
            return null;
        }

        var queryContext = new AutypoQueryContext(
            rawQuery,
            query,
            indexConfiguration.Name,
            extractedQueryTokens: analysisResult.ExtractedTokens,
            transformedQueryTokens: tokens,
            tokenizedLength,
            searchContext.Metadata,
            documentMetadata: documentMetadata,
            indexedDocumentCount: indexedDocumentCount,
            indexedDocumentKeysCount: indexedDocumentKeysCount);

        var partialMatchPolicy = indexConfiguration.CalculatePartialTokenMatchingPolicy is null
            ? PartialMatchPolicy.AllQueryTokensRequired()
            : indexConfiguration.CalculatePartialTokenMatchingPolicy(tokens, queryContext);

        var tokenOrdering = indexConfiguration.TokenOrderingSelector(queryContext);

        if (partialMatchPolicy.Threshold < 1f && tokenOrdering is not TokenOrdering.Unordered)
        {
            throw new AutypoConfigurationException(Resources.AutypoConfiguration_PartialMatching_ReorderingDisabled);
        }

        var tokenMatchAccumulator = new TokenMatchAccumulator(tokenOrdering, partialMatchPolicy);
        var tokenStatsAccumulator = new TokenStatsAccumulator();

        var transformedTokenInfo = new QuerySearchTokenInfo[tokens.Length];
        Debug.Assert(tokens != analysisResult.ExtractedTokens || tokens.Length == analysisResult.TokenizedLength);
        var extractedTokenInfo = tokens == analysisResult.ExtractedTokens ? null : new QuerySearchTokenInfo[analysisResult.TokenizedLength];

        for (int sequenceNumber = 0; sequenceNumber < analysisResult.TokenizedLength; sequenceNumber++)
        {
            if (sequenceNumber >= MaxTokensPerDocument)
            {
                break;
            }

            tokenMatchAccumulator.Prepare(sequenceNumber);

            for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {
                var token = tokens[tokenIndex];
                if (!token.Contains(sequenceNumber))
                {
                    continue;
                }

                tokenStatsAccumulator.Reset();

                var fuzziness = indexConfiguration.FuzzinessSelector(token, queryContext);
                var scope = indexConfiguration.MatchScopeSelector(token, queryContext);

                var metric = fuzziness.AllowTransposition ? LevenshtypoMetric.RestrictedEdit : LevenshtypoMetric.Levenshtein;

                var tokenTerm = new string(token.Text.Span);

                var trieSearchResults = scope switch
                {
                    MatchScope.Full => trie.EnumerateSearch(tokenTerm, fuzziness.MaxEditDistance, metric),
                    MatchScope.Prefix => trie.EnumerateSearchByPrefix(tokenTerm, fuzziness.MaxEditDistance, metric),
                    _ => throw new NotSupportedException($"Match scope {scope} is not supported.")
                };

                foreach (var trieSearchResult in trieSearchResults)
                {
                    var hit = trieSearchResult.Result;

                    var currentDocumentMetadata = indexDocumentMetadata[hit.DocumentIndex];

                    var suffixLength = trieSearchResult.TryGetPrefixSearchMetadata(out var prefixMetadata) ? prefixMetadata.SuffixLength : 0;

                    if (token.SequenceStart == sequenceNumber)
                    {
                        var keyMetadata = currentDocumentMetadata.GetMetadata(indexKeyDocumentMetadata)[hit.KeyNum];

                        tokenMatchAccumulator.Process(
                            hit.DocumentIndex,
                            hit.KeyNum,
                            hit.MatchStartBitmap,
                            hit.MatchLength,
                            trieSearchResult.Distance,
                            suffixLength,
                            keyMetadata.SkippedBitmap);
                    }
                    else
                    {
                        tokenMatchAccumulator.Extend(
                            hit.DocumentIndex,
                            hit.KeyNum,
                            hit.MatchStartBitmap,
                            hit.MatchLength,
                            trieSearchResult.Distance,
                            suffixLength);
                    }
                }

                transformedTokenInfo[tokenIndex] = new QuerySearchTokenInfo
                {
                    MatchScope = scope,
                    Fuzziness = fuzziness,
                    Stats = tokenStatsAccumulator.GetStats()
                };

                if (extractedTokenInfo is not null && token.IsOriginal)
                {
                    Debug.Assert(token.SequenceLength is 1);
                    extractedTokenInfo[token.SequenceStart] = transformedTokenInfo[tokenIndex];
                }
            }
        }

        return new IndexReaderMatches
        {
            QueryContext = queryContext,
            Matches = tokenMatchAccumulator.GetAccumulatedEvidence(),
            IndexDocumentMetadata = indexDocumentMetadata,
            IndexKeyDocumentMetadata = indexKeyDocumentMetadata,
            QuerySearchInfo = new QuerySearchInfo
            {
                PartialMatchPolicy = partialMatchPolicy,
                TokenOrdering = tokenOrdering,
                TransformedTokenInfo = transformedTokenInfo,
                ExtractedTokenInfo = extractedTokenInfo ?? transformedTokenInfo
            }
        };
    }
}
