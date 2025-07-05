using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Autypo;

/// <summary>
/// A match ranker that retains the top <c>N</c> highest scoring matches.
/// </summary>
internal sealed class BoundedMatchRanker(int maxResults) : IMatchRanker
{
    private readonly PriorityQueue<RankedDocument, float> _queue = new();
    private readonly Dictionary<int, RankedDocument> _bestDocs = new();


    /// <inheritdoc/>
    public void Process(int documentIndex, float score, AutypoTags tags)
    {
        Process(new RankedDocument(documentIndex, score, tags));
    }


    /// <inheritdoc/>
    private void Process(RankedDocument document)
    {
        var queue = _queue;
        var scores = _bestDocs;

        bool stop;

        do
        {
            stop = true;

            ref var existing = ref CollectionsMarshal.GetValueRefOrNullRef(scores, document.DocumentIndex);
            if (!Unsafe.IsNullRef(ref existing))
            {
                // The document is already amongst the top.
                // We leave it there and if it ever escapes
                // we re-enqueue it fairly

                if (existing.Score < document.Score)
                {
                    existing = document;
                }
            }
            else
            {
                if (queue.Count < maxResults)
                {
                    queue.Enqueue(document, document.Score);
                    scores.Add(document.DocumentIndex, document);
                }
                else if (queue.TryPeek(out var peeked, out var peekedScore) && peekedScore < document.Score)
                {
                    var dq = queue.EnqueueDequeue(document, document.Score);
                    Debug.Assert(dq.DocumentIndex == peeked.DocumentIndex);
                    scores.Add(document.DocumentIndex, document);

                    var dqDocument = scores[dq.DocumentIndex];
                    scores.Remove(dq.DocumentIndex);
                    if (dqDocument.Score > dq.Score)
                    {
                        // We've removed a document which had a "better" version.
                        // Give that version a fair chance.
                        document = dqDocument;
                        stop = false;
                    }
                }
            }
        } while (!stop);
    }

    /// <inheritdoc/>
    public int Count => _bestDocs.Count;

    /// <inheritdoc/>
    public IEnumerable<RankedDocument> GetRankedDocuments()
    {
        var results = new RankedDocument[_queue.Count];
        var index = results.Length;
        while (_queue.TryDequeue(out var rankedDocument, out _))
        {
            results[--index] = _bestDocs[rankedDocument.DocumentIndex];
        }
        Debug.Assert(index == 0);
        return results;
    }
}
