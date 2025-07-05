using System.Numerics;
using Shouldly;

namespace Autypo.UnitTests;

public class TokenMatchAccumulatorTests
{
    [Theory]
    [MemberData(nameof(GetTestParameters))]
    public void When_single_token_query_matches_exactly(TokenOrdering ordering, PartialMatchPolicy policy)
    {
        var collector = new TokenMatchAccumulator(ordering, policy);

        collector.Prepare(0);

        // [1] Match at each singular position
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_00010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_00100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_01000, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 4, keyNum: 0, matchStartBitmap: 0b_10000, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [2] Match at many singular positions
        collector.Process(documentIndex: 5, keyNum: 0, matchStartBitmap: 0b_10101, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 6, keyNum: 0, matchStartBitmap: 0b_01010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [3] Match against compound tokens
        collector.Process(documentIndex: 7, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 8, keyNum: 0, matchStartBitmap: 0b_00110, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, ordering, policy, numQueryTokens: 1);

        // [1] Match at each singular position
        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_1, dMatch: 0b_00001, dSeq: 0b_00001, dOrd: 0b_00001, qFinal: 0b_00001, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 1, keyNum: 0, qMatch: 0b_1, dMatch: 0b_00010, dSeq: 0b_00010, dOrd: 0b_00010, qFinal: 0b_00010, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 2, keyNum: 0, qMatch: 0b_1, dMatch: 0b_00100, dSeq: 0b_00100, dOrd: 0b_00100, qFinal: 0b_00100, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 3, keyNum: 0, qMatch: 0b_1, dMatch: 0b_01000, dSeq: 0b_01000, dOrd: 0b_01000, qFinal: 0b_01000, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 4, keyNum: 0, qMatch: 0b_1, dMatch: 0b_10000, dSeq: 0b_10000, dOrd: 0b_10000, qFinal: 0b_10000, qSeq: 0b_1, qOrd: 0b_1);

        // [2] Match at many singular positions
        asserter.Assert(documentIndex: 5, keyNum: 0, qMatch: 0b_1, dMatch: 0b_10101, dSeq: 0b_10101, dOrd: 0b_10101, qFinal: 0b_10101, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 6, keyNum: 0, qMatch: 0b_1, dMatch: 0b_01010, dSeq: 0b_01010, dOrd: 0b_01010, qFinal: 0b_01010, qSeq: 0b_1, qOrd: 0b_1);

        // [3] Match against compound tokens
        asserter.Assert(documentIndex: 7, keyNum: 0, qMatch: 0b_1, dMatch: 0b_00011, dSeq: 0b_00010, dOrd: 0b_00010, qFinal: 0b_00011, qSeq: 0b_1, qOrd: 0b_1);
        asserter.Assert(documentIndex: 8, keyNum: 0, qMatch: 0b_1, dMatch: 0b_01110, dSeq: 0b_01100, dOrd: 0b_01100, qFinal: 0b_01110, qSeq: 0b_1, qOrd: 0b_1);
    }

    [Theory]
    [MemberData(nameof(GetTestParameters))]
    public void When_two_token_query_matches_exactly(TokenOrdering ordering, PartialMatchPolicy policy)
    {
        var collector = new TokenMatchAccumulator(ordering, policy);

        collector.Prepare(0);

        // [1] Match at each singular position
        collector.Process(documentIndex: 00, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 01, keyNum: 0, matchStartBitmap: 0b_00010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 02, keyNum: 0, matchStartBitmap: 0b_00100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 03, keyNum: 0, matchStartBitmap: 0b_01000, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        // Document index with no match: 04

        // [2] Match at many singular positions
        collector.Process(documentIndex: 05, keyNum: 0, matchStartBitmap: 0b_00101, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 06, keyNum: 0, matchStartBitmap: 0b_11011, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [3] Match against compound tokens (both first and second)
        collector.Process(documentIndex: 07, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 08, keyNum: 0, matchStartBitmap: 0b_00101, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [4] Match against compound tokens (first only)
        collector.Process(documentIndex: 09, keyNum: 0, matchStartBitmap: 0b_00110, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 10, keyNum: 0, matchStartBitmap: 0b_00111, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [5] Match against compound tokens (second only)
        collector.Process(documentIndex: 11, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 12, keyNum: 0, matchStartBitmap: 0b_00111, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        // [1] Match at each singular position
        collector.Process(documentIndex: 00, keyNum: 0, matchStartBitmap: 0b_00010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 01, keyNum: 0, matchStartBitmap: 0b_00100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 02, keyNum: 0, matchStartBitmap: 0b_01000, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        // Document index with no match: 03
        collector.Process(documentIndex: 04, keyNum: 0, matchStartBitmap: 0b_10000, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [2] Match at many singular positions
        collector.Process(documentIndex: 05, keyNum: 0, matchStartBitmap: 0b_01001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 06, keyNum: 0, matchStartBitmap: 0b_01110, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [3] Match against compound tokens (both first and second)
        collector.Process(documentIndex: 07, keyNum: 0, matchStartBitmap: 0b_00100, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 08, keyNum: 0, matchStartBitmap: 0b_11110, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [4] Match against compound tokens (first only)
        collector.Process(documentIndex: 09, keyNum: 0, matchStartBitmap: 0b_110001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 10, keyNum: 0, matchStartBitmap: 0b_100100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        // [5] Match against compound tokens (second only)
        collector.Process(documentIndex: 11, keyNum: 0, matchStartBitmap: 0b_00010, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 12, keyNum: 0, matchStartBitmap: 0b_00111, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, ordering, policy, numQueryTokens: 2);

        // [1] Match at each singular position
        asserter.Assert(documentIndex: 00, keyNum: 0, qMatch: 0b_11, dMatch: 0b_00011, dSeq: 0b_00010, dOrd: 0b_00010, qFinal: 0b_00010, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 01, keyNum: 0, qMatch: 0b_11, dMatch: 0b_00110, dSeq: 0b_00100, dOrd: 0b_00100, qFinal: 0b_00100, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 02, keyNum: 0, qMatch: 0b_11, dMatch: 0b_01100, dSeq: 0b_01000, dOrd: 0b_01000, qFinal: 0b_01000, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 03, keyNum: 0, qMatch: 0b_01, dMatch: 0b_01000, dSeq: 0b_00000, dOrd: 0b_00000, qFinal: 0b_00000, qSeq: 0b01, qOrd: 0b01);
        asserter.Assert(documentIndex: 04, keyNum: 0, qMatch: 0b_10, dMatch: 0b_10000, dSeq: 0b_00000, dOrd: 0b_00000, qFinal: 0b_10000, qSeq: 0b10, qOrd: 0b10);

        // [2] Match at many singular positions
        asserter.Assert(documentIndex: 05, keyNum: 0, qMatch: 0b_11, dMatch: 0b_01101, dSeq: 0b_01000, dOrd: 0b_01000, qFinal: 0b_01001, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 06, keyNum: 0, qMatch: 0b_11, dMatch: 0b_11111, dSeq: 0b_00110, dOrd: 0b_01110, qFinal: 0b_01110, qSeq: 0b11, qOrd: 0b11);

        // [3] Match against compound tokens (both first and second)
        asserter.Assert(documentIndex: 07, keyNum: 0, qMatch: 0b_11, dMatch: 0b_001111, dSeq: 0b_001000, dOrd: 0b_001000, qFinal: 0b_001100, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 08, keyNum: 0, qMatch: 0b_11, dMatch: 0b_111111, dSeq: 0b_101000, dOrd: 0b_111000, qFinal: 0b_111110, qSeq: 0b11, qOrd: 0b11);

        // [4] Match against compound tokens (first only)
        asserter.Assert(documentIndex: 09, keyNum: 0, qMatch: 0b_11, dMatch: 0b_111111, dSeq: 0b_10000, dOrd: 0b_110000, qFinal: 0b_110001, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 10, keyNum: 0, qMatch: 0b_11, dMatch: 0b_101111, dSeq: 0b_00100, dOrd: 0b_100100, qFinal: 0b_100100, qSeq: 0b11, qOrd: 0b11);

        // [5] Match against compound tokens (second only)
        asserter.Assert(documentIndex: 11, keyNum: 0, qMatch: 0b_11, dMatch: 0b_00111, dSeq: 0b_00100, dOrd: 0b_00100, qFinal: 0b_00110, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 12, keyNum: 0, qMatch: 0b_11, dMatch: 0b_01111, dSeq: 0b_01100, dOrd: 0b_01100, qFinal: 0b_01111, qSeq: 0b11, qOrd: 0b11);
    }

    [Fact]
    public void When_document_matches_multiple_keys()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_01, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_10, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_10, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0); // same as before
        collector.Process(documentIndex: 1, keyNum: 1, matchStartBitmap: 0b_01, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0); // different key

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_11, dMatch: 0b_11, dSeq: 0b_10, dOrd: 0b_10, qFinal: 0b_10, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 1, keyNum: 0, qMatch: 0b_01, dMatch: 0b_10, dSeq: 0b_00, dOrd: 0b_00, qFinal: 0b_00, qSeq: 0b01, qOrd: 0b01);
        asserter.Assert(documentIndex: 1, keyNum: 1, qMatch: 0b_10, dMatch: 0b_01, dSeq: 0b_00, dOrd: 0b_00, qFinal: 0b_01, qSeq: 0b10, qOrd: 0b10);
    }

    [Fact]
    public void When_documental_token_is_missing_then_matching_works()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(2);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 3);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_110, dMatch: 0b_110, dSeq: 0b_000, dOrd: 0b_000, qFinal: 0b_100, qSeq: 0b110, qOrd: 0b110);
        asserter.Assert(documentIndex: 1, keyNum: 0, qMatch: 0b_101, dMatch: 0b_101, dSeq: 0b_000, dOrd: 0b_000, qFinal: 0b_100, qSeq: 0b001, qOrd: 0b001);
        asserter.Assert(documentIndex: 2, keyNum: 0, qMatch: 0b_011, dMatch: 0b_011, dSeq: 0b_000, dOrd: 0b_000, qFinal: 0b_000, qSeq: 0b011, qOrd: 0b011);
        asserter.Assert(documentIndex: 3, keyNum: 0, qMatch: 0b_101, dMatch: 0b_100, dSeq: 0b_000, dOrd: 0b_000, qFinal: 0b_100, qSeq: 0b001, qOrd: 0b001);
    }

    [Fact]
    public void When_token_matches_the_same_document_multiple_times()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_000001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_000110, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_001000, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_000110, matchLength: 2, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_100010, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_11, dMatch: 0b_111111, dSeq: 0b_101110, dOrd: 0b_101110, qFinal: 0b_101110, qSeq: 0b11, qOrd: 0b11);
    }

    [Fact]
    public void When_token_matches_but_not_exactly()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0001, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0010, matchLength: 1, distance: 1, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0100, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0010, matchLength: 1, distance: 1, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0110, matchLength: 1, distance: 0, skipBitmap: 0, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_1001, matchLength: 1, distance: 2, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0,
            qMatch: 0b_11, dMatch: 0b_0111, dSeq: 0b_0010, dOrd: 0b_0110, qFinal: 0b_0110, qSeq: 0b11, qOrd: 0b11,
            qMatch1: 0b_11, dMatch1: 0b_0111, dSeq1: 0b_0110, dOrd1: 0b_0110, qFinal1: 0b_0110, qSeq1: 0b11, qOrd1: 0b11,
            qMatch2: 0b_11, dMatch2: 0b_1111, dSeq2: 0b_1110, dOrd2: 0b_1110, qFinal2: 0b_1111, qSeq2: 0b11, qOrd2: 0b11);
    }

    [Fact]
    public void When_first_token_is_near_but_second_is_fuzzy()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0001, matchLength: 1, distance: 1, skipBitmap: 0, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_0010, matchLength: 1, distance: 2, skipBitmap: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0,
            qMatch: 0b_00, dMatch: 0b_0000, dSeq: 0b_0000, dOrd: 0b_0000, qFinal: 0b_0000, qSeq: 0b00, qOrd: 0b00,
            qMatch1: 0b_01, dMatch1: 0b_0001, dSeq1: 0b_0000, dOrd1: 0b_0000, qFinal1: 0b_0000, qSeq1: 0b01, qOrd1: 0b01,
            qMatch2: 0b_11, dMatch2: 0b_0011, dSeq2: 0b_0010, dOrd2: 0b_0010, qFinal2: 0b_0010, qSeq2: 0b11, qOrd2: 0b11);
    }

    [Fact]
    public void When_token_is_unmatchable_then_sequential_matcher_skips_it()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 1, distance: 0, skipBitmap: 0b00010, suffixLength: 0);
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_00001, matchLength: 1, distance: 0, skipBitmap: 0b10110, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_00011, matchLength: 1, distance: 0, skipBitmap: 0b00110, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_10101, matchLength: 1, distance: 0, skipBitmap: 0b00010, suffixLength: 0); // skip 1
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_11100, matchLength: 1, distance: 0, skipBitmap: 0b10110, suffixLength: 0); // skip 2
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_11000, matchLength: 1, distance: 0, skipBitmap: 0b00110, suffixLength: 0); // only skip where it makes sense

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_11, dMatch: 0b_10101, dSeq: 0b_00100, dOrd: 0b_10100, qFinal: 0b_10101, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 1, keyNum: 0, qMatch: 0b_11, dMatch: 0b_11101, dSeq: 0b_01100, dOrd: 0b_11100, qFinal: 0b_11100, qSeq: 0b11, qOrd: 0b11);
        asserter.Assert(documentIndex: 2, keyNum: 0, qMatch: 0b_11, dMatch: 0b_11011, dSeq: 0b_01000, dOrd: 0b_11000, qFinal: 0b_11000, qSeq: 0b11, qOrd: 0b11);
    }

    [Fact]
    public void When_query_token_is_missed_then_next_token_is_not_In_Order()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        collector.Prepare(1);

        // Not matched

        collector.Prepare(2);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 3);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_101, dMatch: 0b_101, dSeq: 0b_000, dOrd: 0b_000, qFinal: 0b_100, qSeq: 0b001, qOrd: 0b001);
    }

    [Fact]
    public void When_single_query_token_is_prefix_matched_then_suffix_length_is_stored()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_1, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 2);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 1);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_1, dMatch: 0b_1, dSeq: 0b_1, dOrd: 0b_1, qFinal: 0b_1, qSeq: 0b1, qOrd: 0b1, finalSuffix: 2);
    }

    [Fact]
    public void When_query_token_is_prefix_matched_then_final_suffix_length_is_stored()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_01, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 2);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_10, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 3);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.Unordered, PartialMatchPolicy.SomeQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_11, dMatch: 0b_11, dSeq: 0b_10, dOrd: 0b_10, qFinal: 0b_10, qSeq: 0b11, qOrd: 0b11, finalSuffix: 3);
    }

    [Fact]
    public void When_query_compound_token_matches_then_in_order_matching_works()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.InOrder, PartialMatchPolicy.AllQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Process(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 2, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Process(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 3, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Process(documentIndex: 4, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        collector.Prepare(1);

        collector.Extend(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, suffixLength: 0);
        collector.Extend(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, suffixLength: 0);
        collector.Extend(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 2, distance: 0, suffixLength: 0);
        collector.Extend(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 3, distance: 0, suffixLength: 0);
        collector.Process(documentIndex: 4, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 2, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        collector.Prepare(2);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Extend(documentIndex: 1, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, suffixLength: 0);
        collector.Process(documentIndex: 2, keyNum: 0, matchStartBitmap: 0b_100, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Extend(documentIndex: 3, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 3, distance: 0, suffixLength: 0);
        collector.Extend(documentIndex: 4, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 2, distance: 0, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.InOrder, PartialMatchPolicy.AllQueryTokensRequired(), numQueryTokens: 3);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_111, dMatch: 0b_101, dSeq: 0b_000, dOrd: 0b_100, qFinal: 0b_100, qSeq: 0b011, qOrd: 0b111, finalSuffix: 0);
        asserter.Assert(documentIndex: 1, keyNum: 0, qMatch: 0b_111, dMatch: 0b_001, dSeq: 0b_001, dOrd: 0b_001, qFinal: 0b_001, qSeq: 0b111, qOrd: 0b111, finalSuffix: 0);
        asserter.Assert(documentIndex: 2, keyNum: 0, qMatch: 0b_111, dMatch: 0b_111, dSeq: 0b_100, dOrd: 0b_100, qFinal: 0b_100, qSeq: 0b111, qOrd: 0b111, finalSuffix: 0);
        asserter.Assert(documentIndex: 3, keyNum: 0, qMatch: 0b_111, dMatch: 0b_111, dSeq: 0b_100, dOrd: 0b_100, qFinal: 0b_111, qSeq: 0b111, qOrd: 0b111, finalSuffix: 0);
        asserter.Assert(documentIndex: 4, keyNum: 0, qMatch: 0b_111, dMatch: 0b_111, dSeq: 0b_100, dOrd: 0b_100, qFinal: 0b_110, qSeq: 0b111, qOrd: 0b111, finalSuffix: 0);
    }

    [Fact]
    public void When_document_compound_token_matches_after_single_token_match_then_final_query_match_remains_good()
    {
        var collector = new TokenMatchAccumulator(TokenOrdering.InOrder, PartialMatchPolicy.AllQueryTokensRequired());

        collector.Prepare(0);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_001, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        collector.Prepare(1);

        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 1, distance: 0, skipBitmap: 0ul, suffixLength: 0);
        collector.Process(documentIndex: 0, keyNum: 0, matchStartBitmap: 0b_010, matchLength: 2, distance: 0, skipBitmap: 0ul, suffixLength: 0);

        var results = collector.GetAccumulatedEvidence().OrderBy(x => x.DocumentIndex).ThenBy(x => x.KeyNum).ToArray();

        var asserter = new Asserter(results, TokenOrdering.InOrder, PartialMatchPolicy.AllQueryTokensRequired(), numQueryTokens: 2);

        asserter.Assert(documentIndex: 0, keyNum: 0, qMatch: 0b_11, dMatch: 0b_111, dSeq: 0b_110, dOrd: 0b_110, qFinal: 0b_110, qSeq: 0b11, qOrd: 0b11, finalSuffix: 0);
    }

    private class Asserter(
        TokenMatchEvidence[] results,
        TokenOrdering ordering,
        PartialMatchPolicy partialMatchPolicy,
        int numQueryTokens)
    {
        public void Assert(
            int documentIndex,
            int keyNum,
            ulong qMatch,
            ulong dMatch,
            ulong dSeq,
            ulong dOrd,
            ulong qFinal,
            ulong qSeq,
            ulong qOrd,
            byte finalSuffix = 0,
            ulong? qMatch1 = null,
            ulong? qMatch2 = null,
            ulong? dMatch1 = null,
            ulong? dSeq1 = null,
            ulong? dOrd1 = null,
            ulong? qFinal1 = null,
            ulong? qSeq1 = null,
            ulong? qOrd1 = null,
            byte? finalSuffix1 = null,
            ulong? dMatch2 = null,
            ulong? dSeq2 = null,
            ulong? dOrd2 = null,
            ulong? qFinal2 = null,
            ulong? qSeq2 = null,
            ulong? qOrd2 = null,
            byte? finalSuffix2 = null)
        {
            qMatch1 ??= qMatch;
            dMatch1 ??= dMatch;
            dSeq1 ??= dSeq;
            dOrd1 ??= dOrd;
            qFinal1 ??= qFinal;
            qSeq1 ??= qSeq;
            qOrd1 ??= qOrd;
            finalSuffix1 ??= finalSuffix;

            qMatch2 ??= qMatch1;
            dMatch2 ??= dMatch1;
            dSeq2 ??= dSeq1;
            dOrd2 ??= dOrd1;
            qFinal2 ??= qFinal1;
            qSeq2 ??= qSeq1;
            qOrd2 ??= qOrd1;
            finalSuffix2 ??= finalSuffix1;

            var matching = ordering switch
            {
                TokenOrdering.StrictSequence => dSeq2 is not 0,
                TokenOrdering.InOrder => dOrd2 is not 0,
                _ => true
            };

            matching &= BitOperations.PopCount(qMatch2.Value) / (float)numQueryTokens >= partialMatchPolicy.Threshold;
            matching &= (partialMatchPolicy.RequiredTokenMask & qMatch2.Value) == partialMatchPolicy.RequiredTokenMask;

            if (!matching)
            {
                results.ShouldNotContain(r => r.DocumentIndex == documentIndex && r.KeyNum == keyNum);
                return;
            }

            var result = results.Single(r => r.DocumentIndex == documentIndex && r.KeyNum == keyNum);

            result.DocumentIndex.ShouldBe(documentIndex);
            result.KeyNum.ShouldBe(keyNum);

            result.Exact.QueryMatchBitmap.ShouldBe(qMatch);
            result.Exact.DocumentMatchBitmap.ShouldBe(dMatch);
            result.Exact.DocumentSequentialMatchBitmap.ShouldBe(dSeq);
            result.Exact.DocumentInOrderMatchBitmap.ShouldBe(dOrd);
            result.Exact.QueryFinalTokenMatchBitmap.ShouldBe(qFinal);
            result.Exact.QuerySequentialMatchBitmap.ShouldBe(qSeq);
            result.Exact.QueryInOrderMatchBitmap.ShouldBe(qOrd);
            if ((result.Exact.QueryMatchBitmap & (1ul << numQueryTokens)) is not 0ul)
            {
                result.Exact.QueryFinalTokenBestSuffixLength.ShouldBe(finalSuffix);
            }

            result.Near.QueryMatchBitmap.ShouldBe(qMatch1.Value);
            result.Near.DocumentMatchBitmap.ShouldBe(dMatch1.Value);
            result.Near.DocumentSequentialMatchBitmap.ShouldBe(dSeq1.Value);
            result.Near.DocumentInOrderMatchBitmap.ShouldBe(dOrd1.Value);
            result.Near.QueryFinalTokenMatchBitmap.ShouldBe(qFinal1.Value);
            result.Near.QuerySequentialMatchBitmap.ShouldBe(qSeq1.Value);
            result.Near.QueryInOrderMatchBitmap.ShouldBe(qOrd1.Value);
            if ((result.Near.QueryMatchBitmap & (1ul << numQueryTokens)) is not 0ul)
            {
                result.Near.QueryFinalTokenBestSuffixLength.ShouldBe(finalSuffix1.Value);
            }

            result.Fuzzy.QueryMatchBitmap.ShouldBe(qMatch2.Value);
            result.Fuzzy.DocumentMatchBitmap.ShouldBe(dMatch2.Value);
            result.Fuzzy.DocumentSequentialMatchBitmap.ShouldBe(dSeq2.Value);
            result.Fuzzy.DocumentInOrderMatchBitmap.ShouldBe(dOrd2.Value);
            result.Fuzzy.QueryFinalTokenMatchBitmap.ShouldBe(qFinal2.Value);
            result.Fuzzy.QuerySequentialMatchBitmap.ShouldBe(qSeq2.Value);
            result.Fuzzy.QueryInOrderMatchBitmap.ShouldBe(qOrd2.Value);
            if ((result.Fuzzy.QueryMatchBitmap & (1ul << numQueryTokens)) is not 0ul)
            {
                result.Fuzzy.QueryFinalTokenBestSuffixLength.ShouldBe(finalSuffix2.Value);
            }
        }
    }

    public static IEnumerable<object[]> GetTestParameters()
    {
        yield return [TokenOrdering.InOrder, PartialMatchPolicy.SomeQueryTokensRequired(threshold: 0.5f).WithRequiredQueryToken(1)];

        yield return [TokenOrdering.StrictSequence, PartialMatchPolicy.AllQueryTokensRequired()];

        yield return [TokenOrdering.InOrder, PartialMatchPolicy.AllQueryTokensRequired()];

        for (int i = 0; i <= 2; i++)
        {
            yield return [TokenOrdering.InOrder, new PartialMatchPolicy(threshold: 0.75f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.InOrder, new PartialMatchPolicy(threshold: 0.5f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.InOrder, new PartialMatchPolicy(threshold: 0.25f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.InOrder, new PartialMatchPolicy(threshold: 0.00f, requiredTokenMask: (ulong)i)];
        }

        yield return [TokenOrdering.Unordered, PartialMatchPolicy.AllQueryTokensRequired()];

        for (int i = 0; i <= 2; i++)
        {
            yield return [TokenOrdering.Unordered, new PartialMatchPolicy(threshold: 0.75f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.Unordered, new PartialMatchPolicy(threshold: 0.5f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.Unordered, new PartialMatchPolicy(threshold: 0.25f, requiredTokenMask: (ulong)i)];
            yield return [TokenOrdering.Unordered, new PartialMatchPolicy(threshold: 0.00f, requiredTokenMask: (ulong)i)];
        }
    }

}
