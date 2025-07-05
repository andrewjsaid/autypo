using System.Numerics;
using System.Runtime.InteropServices;

namespace Autypo;

/// <summary>
/// Accumulates token match evidence across sequential query tokens,
/// enforcing token ordering and partial match constraints.
/// </summary>
internal class TokenMatchAccumulator(TokenOrdering tokenOrdering, PartialMatchPolicy partialMatchPolicy)
{
    private Dictionary<MatchKey, TokenMatchEvidence> _matchesPrev = new();
    private Dictionary<MatchKey, TokenMatchEvidence> _matchesNext = new();

    private int _sequenceNumber = ~0;
    private bool _isPrevSequenceNumberOptional;

    /// <summary>
    /// Prepares internal state for the next token in the query sequence.
    /// Must be called once per query token before <see cref="Process"/> or <see cref="Extend"/>.
    /// </summary>
    public void Prepare(int sequenceNumber)
    {
        var matchesPrev = _matchesPrev;
        var matchesNext = _matchesNext;

        var prevSequenceNumber = _sequenceNumber;
        if (prevSequenceNumber is ~0)
        {
            _isPrevSequenceNumberOptional = true;
        }
        else
        {
            _isPrevSequenceNumberOptional = partialMatchPolicy.Threshold is not 1f && (partialMatchPolicy.RequiredTokenMask & unchecked(1ul << (int)prevSequenceNumber)) is 0ul;
            if (_isPrevSequenceNumberOptional)
            {
                foreach (var item in matchesPrev)
                {
                    ref var newEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(matchesNext, item.Key, out var exists);
                    if (!exists)
                    {
                        newEntry = item.Value;

                        newEntry.Exact.DocumentSequentialMatchBitmap = 0ul;
                        newEntry.Near.DocumentSequentialMatchBitmap = 0ul;
                        newEntry.Fuzzy.DocumentSequentialMatchBitmap = 0ul;

                        newEntry.Exact.DocumentInOrderMatchBitmap = 0ul;
                        newEntry.Near.DocumentInOrderMatchBitmap = 0ul;
                        newEntry.Fuzzy.DocumentInOrderMatchBitmap = 0ul;

                        newEntry.Exact.QueryFinalTokenMatchBitmap = 0ul;
                        newEntry.Near.QueryFinalTokenMatchBitmap = 0ul;
                        newEntry.Fuzzy.QueryFinalTokenMatchBitmap = 0ul;

                        newEntry.Exact.QueryFinalTokenBestSuffixLength = byte.MaxValue;
                        newEntry.Near.QueryFinalTokenBestSuffixLength = byte.MaxValue;
                        newEntry.Fuzzy.QueryFinalTokenBestSuffixLength = byte.MaxValue;

                    }
                }
            }
        }

        matchesPrev.Clear();
        _matchesPrev = matchesNext;
        _matchesNext = matchesPrev;
        _sequenceNumber = sequenceNumber;
    }

    /// <summary>
    /// Adds evidence for a match against the current query token.
    /// </summary>
    public void Process(
        int documentIndex,
        int keyNum,
        ulong matchStartBitmap,
        byte matchLength,
        int distance,
        int suffixLength,
        ulong skipBitmap)
    {
        var matchKey = new MatchKey(documentIndex, keyNum);

        var matchedPrev = _matchesPrev.TryGetValue(matchKey, out var prevEntry);

        if (!_isPrevSequenceNumberOptional)
        {
            if (!matchedPrev)
            {
                return;
            }

            if (tokenOrdering is TokenOrdering.StrictSequence)
            {
                if (prevEntry.Fuzzy.DocumentSequentialMatchBitmap is 0)
                {
                    return;
                }
            }
            else if (tokenOrdering is TokenOrdering.InOrder)
            {
                if (prevEntry.Fuzzy.DocumentInOrderMatchBitmap is 0)
                {
                    return;
                }
            }
        }

        var isFirstQueryToken = _sequenceNumber is 0;
        var isFirstMatchedQueryToken = _sequenceNumber is 0 || !matchedPrev;

        var extendedMatchBitmap = matchStartBitmap;
        for (var i = 1; i < matchLength; i++)
        {
            extendedMatchBitmap |= matchStartBitmap << i;
        }

        int offset = 0;
        while (offset < matchLength - 1)
        {
            TokenMatchEvidence interimEntry = default;
            interimEntry.DocumentIndex = documentIndex;
            interimEntry.KeyNum = keyNum;

            CopyFromPrev(in prevEntry, ref interimEntry);

            UpdateMatch(
                _sequenceNumber,
                in prevEntry,
                ref interimEntry,
                isFirstQueryToken,
                isFirstMatchedQueryToken,
                matchStartBitmap << offset,
                extendedMatchBitmap,
                skipBitmap,
                distance,
                suffixLength);

            prevEntry = interimEntry;
            offset++;
            isFirstQueryToken = false;
            isFirstMatchedQueryToken = false;
        }

        ref var matchEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(_matchesNext, matchKey, out var exists);
        if (!exists)
        {
            matchEntry.DocumentIndex = documentIndex;
            matchEntry.KeyNum = keyNum;

            CopyFromPrev(in prevEntry, ref matchEntry);

            matchEntry.Exact.QueryFinalTokenBestSuffixLength = byte.MaxValue;
            matchEntry.Near.QueryFinalTokenBestSuffixLength = byte.MaxValue;
            matchEntry.Fuzzy.QueryFinalTokenBestSuffixLength = byte.MaxValue;
        }

        UpdateMatch(
            _sequenceNumber,
            in prevEntry,
            ref matchEntry,
            isFirstQueryToken,
            isFirstMatchedQueryToken,
            matchStartBitmap << offset,
            extendedMatchBitmap,
            skipBitmap,
            distance,
            suffixLength);
    }

    /// <summary>
    /// Extends a previously matched range (e.g., prefix match) to include a longer span
    /// and updates suffix length if applicable.
    /// </summary>
    public void Extend(
        int documentIndex,
        int keyNum,
        ulong matchStartBitmap,
        byte matchLength,
        int distance,
        int suffixLength)
    {
        var matchKey = new MatchKey(documentIndex, keyNum);

        var matchedPrev = _matchesPrev.TryGetValue(matchKey, out var prevEntry);
        if (!matchedPrev)
        {
            // May happen if the tokens used different search parameters
            return;
        }

        var finalMatchBitmap = matchStartBitmap << (matchLength - 1);
        ulong extendedMatchBitmap = matchStartBitmap;
        for (int i = 1; i < matchLength; i++)
        {
            extendedMatchBitmap |= matchStartBitmap << i;
        }

        ref var matchEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(_matchesNext, matchKey, out var exists);
        if (!exists)
        {
            matchEntry.DocumentIndex = documentIndex;
            matchEntry.KeyNum = keyNum;

            CopyFromPrev(in prevEntry, ref matchEntry);

            byte suffixLengthByte = (byte)Math.Min(255, suffixLength);
            matchEntry.Exact.QueryFinalTokenBestSuffixLength = suffixLengthByte;
            matchEntry.Near.QueryFinalTokenBestSuffixLength = suffixLengthByte;
            matchEntry.Fuzzy.QueryFinalTokenBestSuffixLength = suffixLengthByte;
        }


        if (distance is 0)
        {
            ExtendMatch(in prevEntry.Exact, ref matchEntry.Exact, _sequenceNumber, finalMatchBitmap, extendedMatchBitmap, suffixLength);
        }

        if (distance <= 1)
        {
            ExtendMatch(in prevEntry.Near, ref matchEntry.Near, _sequenceNumber, finalMatchBitmap, extendedMatchBitmap, suffixLength);
        }

        ExtendMatch(in prevEntry.Fuzzy, ref matchEntry.Fuzzy, _sequenceNumber, finalMatchBitmap, extendedMatchBitmap, suffixLength);

        static void ExtendMatch(
            in TokenMatchDistanceEvidence prevEntry,
            ref TokenMatchDistanceEvidence matchEntry,
            int sequenceNumber,
            ulong matchBitmap,
            ulong extendedMatchBitmap,
            int suffixLength)
        {
            var queryTokenFlag = unchecked(1ul << (int)sequenceNumber);

            matchEntry.QueryMatchBitmap |= queryTokenFlag;
            matchEntry.QueryFinalTokenMatchBitmap |= extendedMatchBitmap;
            matchEntry.QuerySequentialMatchBitmap |= queryTokenFlag;
            matchEntry.QueryInOrderMatchBitmap |= queryTokenFlag;

            if (suffixLength < matchEntry.QueryFinalTokenBestSuffixLength)
            {
                matchEntry.QueryFinalTokenBestSuffixLength = (byte)Math.Min(255, suffixLength);
            }

            matchEntry.DocumentInOrderMatchBitmap |= matchBitmap & prevEntry.DocumentInOrderMatchBitmap;
            matchEntry.DocumentSequentialMatchBitmap |= matchBitmap & prevEntry.DocumentSequentialMatchBitmap;
        }
    }

    private static void CopyFromPrev(in TokenMatchEvidence prevEntry, ref TokenMatchEvidence matchEntry)
    {
        matchEntry.Exact.QueryMatchBitmap = prevEntry.Exact.QueryMatchBitmap;
        matchEntry.Near.QueryMatchBitmap = prevEntry.Near.QueryMatchBitmap;
        matchEntry.Fuzzy.QueryMatchBitmap = prevEntry.Fuzzy.QueryMatchBitmap;

        matchEntry.Exact.DocumentMatchBitmap = prevEntry.Exact.DocumentMatchBitmap;
        matchEntry.Near.DocumentMatchBitmap = prevEntry.Near.DocumentMatchBitmap;
        matchEntry.Fuzzy.DocumentMatchBitmap = prevEntry.Fuzzy.DocumentMatchBitmap;

        matchEntry.Exact.QuerySequentialMatchBitmap = prevEntry.Exact.QuerySequentialMatchBitmap;
        matchEntry.Near.QuerySequentialMatchBitmap = prevEntry.Near.QuerySequentialMatchBitmap;
        matchEntry.Fuzzy.QuerySequentialMatchBitmap = prevEntry.Fuzzy.QuerySequentialMatchBitmap;

        matchEntry.Exact.QueryInOrderMatchBitmap = prevEntry.Exact.QueryInOrderMatchBitmap;
        matchEntry.Near.QueryInOrderMatchBitmap = prevEntry.Near.QueryInOrderMatchBitmap;
        matchEntry.Fuzzy.QueryInOrderMatchBitmap = prevEntry.Fuzzy.QueryInOrderMatchBitmap;
    }

    private static void UpdateMatch(
        int sequenceNumber,
        in TokenMatchEvidence prevEntry,
        ref TokenMatchEvidence matchEntry,
        bool isFirstQueryToken,
        bool isFirstMatchedQueryToken,
        ulong matchBitmap,
        ulong extendedMatchBitmap,
        ulong skipBitmap,
        int editDistance,
        int suffixLength)
    {
        if (editDistance is 0)
        {
            UpdateMatch(in prevEntry.Exact, ref matchEntry.Exact, sequenceNumber, isFirstQueryToken, isFirstMatchedQueryToken, matchBitmap, extendedMatchBitmap, skipBitmap, suffixLength);
        }

        if (editDistance <= 1)
        {
            UpdateMatch(in prevEntry.Near, ref matchEntry.Near, sequenceNumber, isFirstQueryToken, isFirstMatchedQueryToken, matchBitmap, extendedMatchBitmap, skipBitmap, suffixLength);
        }

        UpdateMatch(in prevEntry.Fuzzy, ref matchEntry.Fuzzy, sequenceNumber, isFirstQueryToken, isFirstMatchedQueryToken, matchBitmap, extendedMatchBitmap, skipBitmap, suffixLength);
    }

    private static void UpdateMatch(
        in TokenMatchDistanceEvidence prevEntry,
        ref TokenMatchDistanceEvidence matchEntry,
        int sequenceNumber,
        bool isFirstQueryToken,
        bool isFirstMatchedQueryToken,
        ulong matchBitmap,
        ulong extendedMatchBitmap,
        ulong skipBitmap,
        int suffixLength)
    {
        var queryTokenFlag = unchecked(1ul << (int)sequenceNumber);

        matchEntry.QueryMatchBitmap |= queryTokenFlag;

        matchEntry.DocumentMatchBitmap |= matchBitmap;

        matchEntry.QueryFinalTokenMatchBitmap |= extendedMatchBitmap;

        if (suffixLength < matchEntry.QueryFinalTokenBestSuffixLength)
        {
            matchEntry.QueryFinalTokenBestSuffixLength = (byte)Math.Min(255, suffixLength);
        }

        if (isFirstQueryToken)
        {
            matchEntry.DocumentSequentialMatchBitmap |= matchBitmap;
            matchEntry.DocumentInOrderMatchBitmap |= matchBitmap;
        }
        else
        {
            if (prevEntry.DocumentInOrderMatchBitmap is not 0ul)
            {
                matchEntry.DocumentInOrderMatchBitmap |= matchBitmap & ~((1ul << (BitOperations.TrailingZeroCount(prevEntry.DocumentInOrderMatchBitmap) + 1)) - 1ul);
            }
            matchEntry.DocumentSequentialMatchBitmap |= matchBitmap & ExtendSequentialBitmapAcrossSkippedTokens(prevEntry.DocumentSequentialMatchBitmap, skipBitmap);
        }

        if (isFirstMatchedQueryToken)
        {
            matchEntry.QuerySequentialMatchBitmap |= queryTokenFlag;
            matchEntry.QueryInOrderMatchBitmap |= queryTokenFlag;
        }
        else
        {
            var isSequentialFromPrevToken = (matchBitmap & ExtendSequentialBitmapAcrossSkippedTokens(prevEntry.QueryFinalTokenMatchBitmap, skipBitmap)) is not 0ul;
            if (isSequentialFromPrevToken)
            {
                matchEntry.QuerySequentialMatchBitmap |= queryTokenFlag;
            }

            var isInOrderFromPrevToken = prevEntry.QueryFinalTokenMatchBitmap is not 0ul && (matchBitmap & ~((1ul << (BitOperations.TrailingZeroCount(prevEntry.QueryFinalTokenMatchBitmap) + 1)) - 1ul)) is not 0ul;
            if (isInOrderFromPrevToken)
            {
                matchEntry.QueryInOrderMatchBitmap |= queryTokenFlag;
            }
        }
    }

    private static ulong ExtendSequentialBitmapAcrossSkippedTokens(ulong sequence, ulong skipBitmap)
    {
        ulong nextOp = sequence << 1;
        ulong result = nextOp;

        while ((nextOp & skipBitmap) is not 0ul)
        {
            nextOp = (nextOp << 1);
            result |= nextOp;
            nextOp &= skipBitmap;
        }

        return result;
    }

    /// <summary>
    /// Returns accumulated evidence after processing all query tokens.
    /// </summary>
    public IEnumerable<TokenMatchEvidence> GetAccumulatedEvidence()
    {
        Prepare(_sequenceNumber + 1);

        IEnumerable<TokenMatchEvidence> results = _matchesPrev.Values;

        if (tokenOrdering is TokenOrdering.StrictSequence)
        {
            results = results.Where(v => v.Fuzzy.DocumentSequentialMatchBitmap is not 0);
        }
        else if (tokenOrdering is TokenOrdering.InOrder)
        {
            results = results.Where(v => v.Fuzzy.DocumentInOrderMatchBitmap is not 0);
        }

        if (partialMatchPolicy.Threshold < 1f)
        {
            var threshold = partialMatchPolicy.Threshold;
            var requiredTokenMask = partialMatchPolicy.RequiredTokenMask;

            if (requiredTokenMask is not 0)
            {
                results = results.Where(v =>
                    (v.Fuzzy.QueryMatchBitmap & requiredTokenMask) == requiredTokenMask
                    && BitOperations.PopCount(v.Fuzzy.QueryMatchBitmap) / (float)_sequenceNumber >= threshold);
            }
            else
            {
                results = results.Where(v =>
                    BitOperations.PopCount(v.Fuzzy.QueryMatchBitmap) / (float)_sequenceNumber >= threshold);
            }
        }

        return results;
    }

    private readonly record struct MatchKey(int DocumentIndex, int KeyNum);
}
