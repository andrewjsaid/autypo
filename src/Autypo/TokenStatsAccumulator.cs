using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Autypo;

/// <summary>
/// Accumulates frequency and positional statistics for matched tokens across documents and keys.
/// </summary>
internal sealed class TokenStatsAccumulator
{
    private readonly Dictionary<int, int> _indexToSlotMap = new(); // maps document index to the head of the linked list
    private MatchBitmaps[] _matchBitmaps = []; // linked list of bitmaps
    private int _bitmapsSize; // number of entries in _bitmaps

    /// <summary>
    /// Resets the accumulator state, clearing all stored statistics.
    /// </summary>
    public void Reset()
    {
        _indexToSlotMap.Clear();
        _bitmapsSize = 0;
        Array.Clear(_matchBitmaps);
    }

    /// <summary>
    /// Records a token match at the specified distance and location.
    /// </summary>
    /// <param name="documentIndex">The index of the matched document.</param>
    /// <param name="keyNum">The key number within the document.</param>
    /// <param name="matchStartBitmap">A bitmap indicating the start position of the match.</param>
    /// <param name="distance">The edit distance of the match (0 = exact).</param>
    public void ProcessHit(int documentIndex, int keyNum, ulong matchStartBitmap, int distance)
    {
        ref var bitmaps = ref GetBitmapsRef(documentIndex, keyNum, createIfNotExists: true);

        Debug.Assert(!Unsafe.IsNullRef(ref bitmaps));

        if (distance == 0)
        {
            bitmaps.Exact |= matchStartBitmap;
        }

        if (distance <= 1)
        {
            bitmaps.Near |= matchStartBitmap;
        }

        bitmaps.Fuzzy |= matchStartBitmap;
    }

    private ref MatchBitmaps GetBitmapsRef(int documentIndex, int keyNum, bool createIfNotExists)
    {
        ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_indexToSlotMap, documentIndex, out var exists);

        ref var bitmaps = ref Unsafe.NullRef<MatchBitmaps>();

        if (exists)
        {
            bitmaps = ref _matchBitmaps[slot];
            while (bitmaps.KeyNum != keyNum)
            {
                var nextIndex = bitmaps.NextIndex;
                if (nextIndex is -1)
                {
                    slot = ref bitmaps.NextIndex;
                    exists = false;
                    break;
                }

                bitmaps = ref _matchBitmaps[nextIndex];
            }

            if (exists)
            {
                Debug.Assert(!Unsafe.IsNullRef(ref bitmaps));
                return ref bitmaps;
            }
        }

        if (!createIfNotExists)
        {
            return ref Unsafe.NullRef<MatchBitmaps>();
        }

        EnsureBitmapsHasExtraCapacity();
        slot = _bitmapsSize++;
        bitmaps = ref _matchBitmaps[slot];
        bitmaps = new MatchBitmaps
        {
            KeyNum = keyNum,
            NextIndex = -1
        };
        return ref bitmaps;

    }

    /// <summary>
    /// Computes final token statistics for all processed hits.
    /// </summary>
    /// <returns>A structure containing stats at exact, near, and fuzzy distance levels.</returns>
    public InternalTokenStats GetStats()
    {
        TokenDistanceStatsBuilder exactDistanceStats = new();
        TokenDistanceStatsBuilder nearDistanceStats = new();
        TokenDistanceStatsBuilder fuzzyDistanceStats = new();

        foreach (var documentHead in _indexToSlotMap.Values)
        {
            exactDistanceStats.DocumentFlag = false;
            nearDistanceStats.DocumentFlag = false;
            fuzzyDistanceStats.DocumentFlag = false;

            var bitmapIndex = documentHead;
            while (bitmapIndex is not -1)
            {
                var bitmaps = _matchBitmaps[bitmapIndex];

                TallyStats(bitmaps.Exact, ref exactDistanceStats);
                TallyStats(bitmaps.Near, ref nearDistanceStats);
                TallyStats(bitmaps.Fuzzy, ref fuzzyDistanceStats);

                bitmapIndex = bitmaps.NextIndex;
            }
        }

        return new InternalTokenStats
        {
            ExactStats = exactDistanceStats.Build(),
            NearStats = nearDistanceStats.Build(),
            FuzzyStats = fuzzyDistanceStats.Build()
        };

        static void TallyStats(ulong bitmap, ref TokenDistanceStatsBuilder stats)
        {
            if (bitmap is 0)
            {
                return;
            }

            var currTermFrequency = BitOperations.PopCount(bitmap);
            var currMinPosition = BitOperations.TrailingZeroCount(bitmap);
            var currMaxPosition = 63 - BitOperations.LeadingZeroCount(bitmap);

            if (!stats.DocumentFlag)
            {
                stats.DocumentFlag = true;

                // Per-document stats
                stats.DocumentFrequency++;
            }

            stats.DocumentKeyFrequency++;
            stats.CollectionFrequency += currTermFrequency;
            stats.MinPosition = Math.Min(stats.MinPosition, currMinPosition);
            stats.MaxPosition = Math.Max(stats.MaxPosition, currMaxPosition);
            stats.MaxTermFrequency = Math.Max(stats.MaxTermFrequency, currTermFrequency);
            stats.TotalMinPosition += currMinPosition;
            stats.TotalMaxPosition += currMaxPosition;
        }
    }
    
    private void EnsureBitmapsHasExtraCapacity()
    {
        if (_bitmapsSize == _matchBitmaps.Length)
        {
            Array.Resize(ref _matchBitmaps, _matchBitmaps.Length switch
            {
                < 16 => 16,
                _ => _matchBitmaps.Length * 2
            });
        }
    }

    /// <summary>
    /// Represents positional match bitmaps for a single document key.
    /// </summary>
    private struct MatchBitmaps
    {
        public int KeyNum;
        public int NextIndex;
        public ulong Exact;
        public ulong Near;
        public ulong Fuzzy;
    }

    /// <summary>
    /// Builder struct for collecting and aggregating statistics across multiple keys and documents.
    /// </summary>
    private struct TokenDistanceStatsBuilder
    {
        public bool DocumentFlag;
        public int DocumentFrequency;
        public int DocumentKeyFrequency;
        public int CollectionFrequency;
        public int MaxTermFrequency;
        public int MinPosition;
        public int MaxPosition;
        public int TotalMinPosition;
        public int TotalMaxPosition;

        /// <summary>
        /// Returns a finalized statistics structure for this builder.
        /// </summary>
        public InternalTokenDistanceStats Build()
        {
            return new InternalTokenDistanceStats
            {
                DocumentFrequency = DocumentFrequency,
                DocumentKeyFrequency = DocumentKeyFrequency,
                CollectionFrequency = CollectionFrequency,
                MaxTermFrequency = MaxTermFrequency,
                MinPosition = MinPosition,
                MaxPosition = MaxPosition,
                AverageMinPosition = TotalMinPosition / (float)DocumentKeyFrequency,
                AverageMaxPosition = TotalMaxPosition / (float)DocumentKeyFrequency,
            };
        }
    }
}
