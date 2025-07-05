using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Represents the full query context passed to match selectors, scoring logic,
/// and ranking functions during search execution.
/// </summary>
/// <remarks>
/// <see cref="AutypoQueryContext"/> is constructed after query tokenization and transformation,
/// and provides access to the intermediate and final token representations,
/// along with document counts and caller-provided metadata.
/// </remarks>
public sealed class AutypoQueryContext
{
    private static readonly IReadOnlyDictionary<string, object> _emptyMetadata = new Dictionary<string, object>();
    private readonly Array _documentMetadata;

    internal AutypoQueryContext(
        string rawQuery,
        string query,
        string? indexName,
        IReadOnlyList<AutypoToken> extractedQueryTokens,
        IReadOnlyList<AutypoToken> transformedQueryTokens,
        int queryTokenizedLength,
        IReadOnlyDictionary<string, object>? metadata,
        Array documentMetadata,
        int indexedDocumentCount,
        int indexedDocumentKeysCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(queryTokenizedLength, 0);

        RawQuery = rawQuery;
        Query = query;
        IndexName = indexName;
        ExtractedQueryTokens = extractedQueryTokens;
        TransformedQueryTokens = transformedQueryTokens;
        QueryTokenizedLength = queryTokenizedLength;
        Metadata = metadata ?? _emptyMetadata;
        _documentMetadata = documentMetadata;
        IndexedDocumentCount = indexedDocumentCount;
        IndexedDocumentKeysCount = indexedDocumentKeysCount;
    }

    /// <summary>
    /// The original query string provided by the caller.
    /// </summary>
    public string RawQuery { get; }

    /// <summary>
    /// The normalized query string after any pre-tokenization transformations.
    /// </summary>
    public string Query { get; }

    /// <summary>
    /// The name of the index currently being searched.
    /// </summary>
    public string? IndexName { get; }

    /// <summary>
    /// The tokens extracted from the normalized query prior to transformation.
    /// </summary>
    public IReadOnlyList<AutypoToken> ExtractedQueryTokens { get; }

    /// <summary>
    /// The tokens after applying all configured transformations.
    /// </summary>
    public IReadOnlyList<AutypoToken> TransformedQueryTokens { get; }

    /// <summary>
    /// The number of token positions in the original query.
    /// May differ from the number of transformed tokens.
    /// </summary>
    public int QueryTokenizedLength { get; }

    /// <summary>
    /// Arbitrary caller-supplied metadata available to query-time selectors and filters.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the total number of documents loaded by the global data source.
    /// </summary>
    /// <remarks>
    /// This includes all documents visible to the current index, regardless of filtering logic.
    /// </remarks>
    public int DocumentCount => _documentMetadata.Length;

    /// <summary>
    /// Gets the number of documents indexed into the current index.
    /// </summary>
    public int IndexedDocumentCount { get; }

    /// <summary>
    /// Gets the number of document keys indexed into the current index.
    /// </summary>
    public int IndexedDocumentKeysCount { get; }

    /// <summary>
    /// Gets the type of the indexed documents.
    /// </summary>
    public Type DocumentType => _documentMetadata.GetType().GetElementType()!.GenericTypeArguments[0];

    /// <summary>
    /// Returns all documents loaded by the global data source, including those excluded
    /// from the current index via <c>ShouldIndex</c>.
    /// </summary>
    /// <typeparam name="T">The expected type of the indexed documents.</typeparam>
    /// <returns>
    /// A sequence of all globally loaded documents of type <typeparamref name="T"/>.
    /// Each result includes a score. These may be used for fallback or inspection scenarios,
    /// even if the document was not indexed into the current index.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the type <typeparamref name="T"/> does not match the actual document type used in indexing.
    /// </exception>
    /// <remarks>
    /// This method returns all documents returned by the data loading phase, including those
    /// where the global <c>ShouldIndex</c> function returned <c>true</c> but the per-index
    /// <c>ShouldIndex</c> function returned <c>false</c>.
    /// </remarks>
    public IEnumerable<ScoredDocument<T>> GetDocuments<T>()
    {
        if (_documentMetadata is DocumentMetadata<T>[] typedDocumentMetadata)
        {
            foreach (var document in typedDocumentMetadata)
            {
                yield return new ScoredDocument<T>
                {
                    Document = document.Document,
                    Score = document.Score
                };
            }
        }
        else
        {
            throw new InvalidOperationException(Resources.AutypoQueryContext_Invalid_DocumentType);
        }
    }
}