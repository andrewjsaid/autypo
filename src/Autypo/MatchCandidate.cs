using System.Diagnostics;
using System.Numerics;
using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Represents a candidate document matched against a query, along with rich metadata
/// about how the query tokens and document tokens aligned.
/// </summary>
/// <typeparam name="T">The type of the document stored in the index.</typeparam>
public sealed class MatchCandidate<T>
{
    // SetMatch is always called first so certain fields can start off null

    private TokenMatchEvidence _tokenMatchEvidence;
    private DocumentMetadata<T> _documentMetadata;
    private IndexKeyDocumentMetadata _indexKeyDocumentMetadata;
    private AutypoQueryContext _queryContext = default!;
    private QuerySearchInfo _querySearchStats = default!;
    private AutypoTags _tags;


    internal MatchCandidate()
    {
    }

    internal void SetMatch(
        DocumentMetadata<T>[] documentMetadata,
        IndexDocumentMetadata[] indexDocumentMetadata,
        IndexKeyDocumentMetadata[] indexKeyDocumentMetadata,
        TokenMatchEvidence tokenMatchEvidence,
        AutypoQueryContext queryContext,
        QuerySearchInfo querySearchStats)
    {
        _tokenMatchEvidence = tokenMatchEvidence;
        _documentMetadata = documentMetadata[tokenMatchEvidence.DocumentIndex];
        _indexKeyDocumentMetadata = indexDocumentMetadata[tokenMatchEvidence.DocumentIndex].GetMetadata(indexKeyDocumentMetadata, tokenMatchEvidence.KeyNum);
        _queryContext = queryContext;
        _querySearchStats = querySearchStats;
        _tags = AutypoTags.None;
    }

    /// <summary>
    /// Gets the name of the index where the match was found, or <c>null</c> if unspecified.
    /// </summary>
    public string? IndexName => _queryContext.IndexName;

    /// <summary>
    /// Gets the configured partial match policy used during matching.
    /// </summary>
    public PartialMatchPolicy PartialMatchPolicy => _querySearchStats.PartialMatchPolicy;

    /// <summary>
    /// Gets the configured partial match policy used during matching.
    /// </summary>
    public TokenOrdering TokenOrdering => _querySearchStats.TokenOrdering;

    #region Query Features

    /// <summary>
    /// Gets the number of query tokens produced after tokenization.
    /// </summary>
    public int QueryTokenizedLength => _queryContext.QueryTokenizedLength;

    /// <summary>
    /// Gets the number of query tokens that matched this candidate exactly (edit distance = 0).
    /// </summary>
    public int QueryExactMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Exact.QueryMatchBitmap);

    /// <summary>
    /// Gets the number of query tokens that matched this candidate exactly (edit distance = 0).
    /// </summary>
    public int QueryNearMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Near.QueryMatchBitmap & ~_tokenMatchEvidence.Exact.QueryMatchBitmap);

    /// <summary>
    /// Gets the number of query tokens that matched this candidate fuzzily (edit distance ≥ 2).
    /// </summary>
    public int QueryFuzzyMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Fuzzy.QueryMatchBitmap & ~_tokenMatchEvidence.Near.QueryMatchBitmap & ~_tokenMatchEvidence.Exact.QueryMatchBitmap);

    /// <summary>
    /// Gets the number of sequential exact matches from the query.
    /// </summary>
    public int QueryExactSequentialMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Exact.QuerySequentialMatchBitmap);

    /// <summary>
    /// Gets the number of sequential near matches from the query.
    /// </summary>
    public int QueryNearSequentialMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Near.QuerySequentialMatchBitmap & ~_tokenMatchEvidence.Exact.QuerySequentialMatchBitmap);

    /// <summary>
    /// Gets the number of sequential fuzzy matches from the query.
    /// </summary>
    public int QueryFuzzySequentialMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Fuzzy.QuerySequentialMatchBitmap & ~_tokenMatchEvidence.Near.QuerySequentialMatchBitmap & ~_tokenMatchEvidence.Exact.QuerySequentialMatchBitmap);

    /// <summary>
    /// Gets the number of in-order exact matches from the query.
    /// </summary>
    public int QueryExactInOrderMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Exact.QueryInOrderMatchBitmap);

    /// <summary>
    /// Gets the number of in-order near matches from the query.
    /// </summary>
    public int QueryNearInOrderMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Near.QueryInOrderMatchBitmap & ~_tokenMatchEvidence.Exact.QueryInOrderMatchBitmap);

    /// <summary>
    /// Gets the number of in-order fuzzy matches from the query.
    /// </summary>
    public int QueryFuzzyInOrderMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Fuzzy.QueryInOrderMatchBitmap & ~_tokenMatchEvidence.Near.QueryInOrderMatchBitmap & ~_tokenMatchEvidence.Exact.QueryInOrderMatchBitmap);

    /// <summary>
    /// Gets the index of the first query token that matched exactly, or the query length if none matched.
    /// </summary>
    public int QueryFirstExactMatchIndex => _tokenMatchEvidence.Exact.QueryMatchBitmap is { } b && b is 0ul ? QueryTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Gets the index of the first query token that matched with minimal fuzziness, or the query length if none matched.
    /// </summary>
    public int QueryFirstNearMatchIndex => (_tokenMatchEvidence.Near.QueryMatchBitmap & ~_tokenMatchEvidence.Exact.QueryMatchBitmap) is { } b && b is 0ul ? QueryTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Gets the index of the first query token that matched with fuzziness ≥ 2, or the query length if none matched.
    /// </summary>
    public int QueryFirstFuzzyMatchIndex => (_tokenMatchEvidence.Fuzzy.QueryMatchBitmap & ~_tokenMatchEvidence.Near.QueryMatchBitmap & ~_tokenMatchEvidence.Exact.QueryMatchBitmap) is { } b && b is 0ul ? QueryTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Gets the best suffix match length for the final query token (exact).
    /// </summary>
    public int QueryFinalExactTokenBestSuffixLength => _tokenMatchEvidence.Exact.QueryFinalTokenBestSuffixLength;

    /// <summary>
    /// Gets the best suffix match length for the final query token (near).
    /// </summary>
    public int QueryFinalNearTokenBestSuffixLength => _tokenMatchEvidence.Near.QueryFinalTokenBestSuffixLength;

    /// <summary>
    /// Gets the best suffix match length for the final query token (fuzzy).
    /// </summary>
    public int QueryFinalFuzzyTokenBestSuffixLength => _tokenMatchEvidence.Fuzzy.QueryFinalTokenBestSuffixLength;
   
    #endregion

    #region Document Features

    /// <summary>
    /// Gets the base score of the document, as supplied at index time.
    /// </summary>
    public float DocumentScore => _documentMetadata.Score;

    /// <summary>
    /// Gets the original document that was matched.
    /// </summary>
    public T Document => _documentMetadata.Document;

    /// <summary>
    /// Gets the number of tokens in the document key that matched this query.
    /// </summary>
    public int DocumentTokenizedLength => _indexKeyDocumentMetadata.TokenizedLength;

    /// <summary>
    /// Gets the number of skipped tokens in the document (e.g., stop words).
    /// </summary>
    public int DocumentSkippedTokenCount => BitOperations.PopCount(_indexKeyDocumentMetadata.SkippedBitmap);

    /// <summary>
    /// Gets the number of document tokens that matched exactly.
    /// </summary>
    public int DocumentExactMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Exact.DocumentMatchBitmap);

    /// <summary>
    /// Gets the number of document tokens that matched with minimal fuzziness.
    /// </summary>
    public int DocumentNearMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Near.DocumentMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentMatchBitmap);

    /// <summary>
    /// Gets the number of document tokens that matched fuzzily.
    /// </summary>
    public int DocumentFuzzyMatchCount => BitOperations.PopCount(_tokenMatchEvidence.Fuzzy.DocumentMatchBitmap & ~_tokenMatchEvidence.Near.DocumentMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentMatchBitmap);

    /// <summary>
    /// Gets the index of the first document token that matched exactly, or the document length if none matched.
    /// </summary>
    public int DocumentFirstExactMatchIndex => _tokenMatchEvidence.Exact.DocumentMatchBitmap is { } b && b is 0ul ? DocumentTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Gets the index of the first document token that matched with minimal fuzziness, or the document length if none matched.
    /// </summary>
    public int DocumentFirstNearMatchIndex => (_tokenMatchEvidence.Near.DocumentMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentMatchBitmap) is { } b && b is 0ul ? DocumentTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Gets the index of the first document token that matched fuzzily, or the document length if none matched.
    /// </summary>
    public int DocumentFirstFuzzyMatchIndex => (_tokenMatchEvidence.Fuzzy.DocumentMatchBitmap & ~_tokenMatchEvidence.Near.DocumentMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentMatchBitmap) is { } b && b is 0ul ? DocumentTokenizedLength : BitOperations.TrailingZeroCount(b);

    /// <summary>
    /// Indicates whether the document has any sequential exact match.
    /// </summary>
    public bool DocumentHasExactSequentialMatch => _tokenMatchEvidence.Exact.DocumentSequentialMatchBitmap is not 0ul;

    /// <summary>
    /// Indicates whether the document has any sequential near match.
    /// </summary>
    public bool DocumentHasNearSequentialMatch => (_tokenMatchEvidence.Near.DocumentSequentialMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentSequentialMatchBitmap) is not 0ul;

    /// <summary>
    /// Indicates whether the document has any sequential fuzzy match.
    /// </summary>
    public bool DocumentHasFuzzySequentialMatch => (_tokenMatchEvidence.Fuzzy.DocumentSequentialMatchBitmap & ~_tokenMatchEvidence.Near.DocumentSequentialMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentSequentialMatchBitmap) is not 0ul;

    /// <summary>
    /// Indicates whether the document has any in-order exact match.
    /// </summary>
    public bool DocumentHasExactInOrderMatch => _tokenMatchEvidence.Exact.DocumentInOrderMatchBitmap is not 0ul;

    /// <summary>
    /// Indicates whether the document has any in-order near match.
    /// </summary>
    public bool DocumentHasNearInOrderMatch => (_tokenMatchEvidence.Near.DocumentInOrderMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentInOrderMatchBitmap) is not 0ul;

    /// <summary>
    /// Indicates whether the document has any in-order fuzzy match.
    /// </summary>
    public bool DocumentHasFuzzyInOrderMatch => (_tokenMatchEvidence.Fuzzy.DocumentInOrderMatchBitmap & ~_tokenMatchEvidence.Near.DocumentInOrderMatchBitmap & ~_tokenMatchEvidence.Exact.DocumentInOrderMatchBitmap) is not 0ul;

    #endregion

    /// <summary>
    /// Gets a mutable reference to the tag set associated with this match.
    /// </summary>
    public ref AutypoTags Tags => ref _tags;

    /// <summary>
    /// Returns detailed token-level information for the specified extracted query token.
    /// </summary>
    /// <param name="index">The token index.</param>
    /// <returns>A <see cref="QueryTokenInfo"/> instance for the token.</returns>
    public QueryTokenInfo GetExtractedQueryTokenInfo(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, QueryTokenizedLength);

        Debug.Assert(_queryContext.ExtractedQueryTokens.Count == QueryTokenizedLength);
        Debug.Assert(_querySearchStats.ExtractedTokenInfo.Length == QueryTokenizedLength);

        return CreateQueryTokenInfo(
            _queryContext.ExtractedQueryTokens[index],
            _querySearchStats.ExtractedTokenInfo[index],
            _tokenMatchEvidence);
    }

    private static QueryTokenInfo CreateQueryTokenInfo(AutypoToken token, QuerySearchTokenInfo stats, TokenMatchEvidence evidence)
    {
        return new QueryTokenInfo
        {
            Token = token,
            MatchedExact = (evidence.Exact.QueryMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedNear = (evidence.Near.QueryMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedFuzzy = (evidence.Fuzzy.QueryMatchBitmap & (1u << token.SequenceStart)) is not 0ul,

            MatchedSequentialExact = (evidence.Exact.QuerySequentialMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedSequentialNear = (evidence.Near.QuerySequentialMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedSequentialFuzzy = (evidence.Fuzzy.QuerySequentialMatchBitmap & (1u << token.SequenceStart)) is not 0ul,

            MatchedInOrderExact = (evidence.Exact.QueryInOrderMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedInOrderNear = (evidence.Near.QueryInOrderMatchBitmap & (1u << token.SequenceStart)) is not 0ul,
            MatchedInOrderFuzzy = (evidence.Fuzzy.QueryInOrderMatchBitmap & (1u << token.SequenceStart)) is not 0ul,

            Fuzziness = stats.Fuzziness,
            MatchScope = stats.MatchScope,
            ExactStats = QueryTokenDistanceStats.MapFrom(stats.Stats.ExactStats),
            NearStats = QueryTokenDistanceStats.MapFrom(stats.Stats.NearStats),
            FuzzyStats = QueryTokenDistanceStats.MapFrom(stats.Stats.FuzzyStats),
        };
    }

    /// <summary>
    /// Returns detailed token-level information for the specified extracted document token.
    /// </summary>
    /// <param name="index">The token index.</param>
    /// <returns>A <see cref="DocumentTokenInfo"/> instance for the token.</returns>
    public DocumentTokenInfo GetExtractedDocumentTokenInfo(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, (int)_indexKeyDocumentMetadata.TokenizedLength);

        return new DocumentTokenInfo
        {
            Token = _indexKeyDocumentMetadata.ExtractedTokens?.Single(t => t.SequenceStart == index && t.SequenceLength == 1),

            IsSkipped = (_indexKeyDocumentMetadata.SkippedBitmap & (1u << index)) is not 0,

            IsExactMatch = (_tokenMatchEvidence.Exact.DocumentMatchBitmap & (1u << index)) is not 0,
            IsNearMatch = (_tokenMatchEvidence.Near.DocumentMatchBitmap & (1u << index)) is not 0,
            IsFuzzyMatch = (_tokenMatchEvidence.Fuzzy.DocumentMatchBitmap & (1u << index)) is not 0,

            EndsExactSequentialMatch = (_tokenMatchEvidence.Exact.DocumentSequentialMatchBitmap & (1u << index)) is not 0,
            EndsNearSequentialMatch = (_tokenMatchEvidence.Near.DocumentSequentialMatchBitmap & (1u << index)) is not 0,
            EndsFuzzySequentialMatch = (_tokenMatchEvidence.Fuzzy.DocumentSequentialMatchBitmap & (1u << index)) is not 0,

            EndsExactInOrderMatch = (_tokenMatchEvidence.Exact.DocumentInOrderMatchBitmap & (1u << index)) is not 0,
            EndsNearInOrderMatch = (_tokenMatchEvidence.Near.DocumentInOrderMatchBitmap & (1u << index)) is not 0,
            EndsFuzzyInOrderMatch = (_tokenMatchEvidence.Fuzzy.DocumentInOrderMatchBitmap & (1u << index)) is not 0
        };
    }
}
