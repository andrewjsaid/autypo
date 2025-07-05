using Autypo.Configuration;
using Autypo.Tokenization;
using Levenshtypo;

namespace Autypo;

/// <summary>
/// Responsible for analyzing and indexing document tokens into the trie-based index.
/// </summary>
/// <typeparam name="T">The type of the document being indexed.</typeparam>
/// <param name="indexConfiguration">The index configuration for this document type.</param>
/// <param name="trie">The trie where tokens are stored and associated with token hits.</param>
/// <param name="keepTokenization">
/// Whether to retain full tokenization information for each document key,
/// enabling diagnostics or advanced scoring.
/// </param>
internal sealed class IndexWriter<T>(
    AutypoIndexConfiguration<T> indexConfiguration,
    LevenshtrieSet<TokenHit> trie,
    bool keepTokenization)
{
    private const int MaxTokensPerDocument = 64;

    private readonly AutypoTextAnalyzer _textAnalyzer = indexConfiguration.DocumentTextAnalyzerFactory();

    /// <summary>
    /// Indexes all keys of the provided document and updates the associated trie and metadata.
    /// </summary>
    /// <param name="document">The document to index.</param>
    /// <param name="documentIndex">The document's unique index in the system.</param>
    /// <param name="metadata">
    /// A list that is appended with token metadata for each indexed key of the document.
    /// </param>
    public void Index(T document, int documentIndex, List<IndexKeyDocumentMetadata> metadata)
    {
        if (indexConfiguration.ShouldIndex is { } shouldIndex && !shouldIndex(document))
        {
            return;
        }

        var keyNum = 0;

        var key = indexConfiguration.KeySelector(document);
        if (key is not null)
        {
            Index(key, documentIndex, keyNum++, metadata);
        }

        if (indexConfiguration.AdditionalKeySelectors is { } additionalKeysSelector)
        {
            var additionalKeys = additionalKeysSelector(document);

            foreach (var additionalKey in additionalKeys)
            {
                Index(additionalKey, documentIndex, keyNum++, metadata);
            }
        }
    }

    /// <summary>
    /// Tokenizes a single key and inserts its tokens into the trie, updating metadata accordingly.
    /// </summary>
    /// <param name="key">The text key extracted from the document.</param>
    /// <param name="documentIndex">The index of the document in the collection.</param>
    /// <param name="keyNum">A sequential identifier for this document key.</param>
    /// <param name="metadata">The metadata list to append to.</param>
    private void Index(string key, int documentIndex, int keyNum, List<IndexKeyDocumentMetadata> metadata)
    {
        var analyzerResult = _textAnalyzer.Analyze(key.AsMemory());
        var tokens = analyzerResult.TransformedTokens;

        foreach (var token in tokens)
        {
            if (token.SequenceStart >= MaxTokensPerDocument)
            {
                break;
            }

            ref var entry = ref trie.GetOrAddRef(
                token.Text.Span,
                new TokenHit
                {
                    DocumentIndex = documentIndex,
                    KeyNum = keyNum,
                    MatchLength = (byte)token.SequenceLength
                },
                exists: out _);

            entry.MatchStartBitmap |= token.SequenceStartBitmap;
        }

        metadata.Add(new IndexKeyDocumentMetadata(
            keyNum: keyNum,
            extractedTokens: keepTokenization ? analyzerResult.ExtractedTokens : null,
            transformedTokens: keepTokenization ? analyzerResult.TransformedTokens : null,
            skippedBitmap: analyzerResult.SkippedBitmap,
            tokenizedLength: analyzerResult.TokenizedLength));
    }
}
