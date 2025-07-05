# 📦 Searching & Matching

Searching & matching is all about configuring Autypo to include the results that make sense in **your** domain.
For example, in some applications, the order of words in the query does not matter — while in others it does.

Out of the box, without any additional configuration:
- A match is returned if **all query words** are found in the document
- Words must appear in the **same order** (but may skip over other tokens)
- Typos are tolerated based on token length
- The final word in the query can match a **prefix** of a word in the document

This guide covers:

- Typo Tolerance
- Prefix Matching
- Token Ordering
- Partial Token Matching
- Word Splitting / Word Joining

After finding all results, Autypo ranks and filters down the result set to find the best matches.
Read our guide on [scoring](./scoring.md) to find out more.

---

## ✏️ Typo Tolerance

Autypo supports fuzzy token matching to tolerate typos during search and autocomplete.

By default, Autypo allows a small number of edits based on token length, using a built-in heuristic.
However, this behavior is fully configurable and can be overridden at both global and per-token levels.

---

### 🔧 Defining Fuzziness

Fuzziness is represented by the struct:

```csharp
struct Fuzziness(int maxEditDistance, bool allowTransposition)
```

It defines:
- `maxEditDistance`: The maximum number of character edits allowed
- `allowTransposition`: Whether to allow character swaps (e.g., "teh" → "the")

Fuzziness is applied per token using a delegate:

```csharp
delegate Fuzziness FuzzinessSelector(AutypoToken token, AutypoQueryContext queryContext);
```

To configure fuzzy matching:

```csharp
.WithFuzziness(FuzzinessSelector selector)
```

---

### ✨ Convenience Methods

For convenience, Autypo provides shortcuts for common cases:

#### ❌ Disable all fuzziness:

```csharp
.WithNoFuzziness()
```

#### 🔁 Apply uniform fuzziness to every token:

```csharp
.WithFuzziness(int maxEditDistance, bool allowTransposition = false)
```

---

### 🧠 Dynamic Fuzziness Based on Query Context

You can adapt fuzziness based on external metadata or runtime conditions:

```csharp
.WithFuzziness((token, context) =>
{
    if (context.Metadata.TryGetValue("fuzz", out var value) && value is int fuzz)
    {
        return new Fuzziness(maxEditDistance: fuzz, allowTransposition: false);
    }

    return Fuzziness.None;
})
```

And pass the metadata at query time:

```csharp
complete.Complete("query", new AutypoSearchContext
{
    Metadata = new Dictionary<string, object>
    {
        { "fuzz", 2 }
    }
});
```

---

## 🔤 Prefix Matching

Autypo supports prefix matching to enable partial queries — especially useful for autocomplete scenarios.

By default, prefix matching is enabled **only on the final token** of the query. This allows for progressive matching as users type.

---

### 🔧 Defining Match Scope

Prefix behavior is controlled via the `MatchScope` enum:

```csharp
enum MatchScope
{
    Full,   // Token must match the entire target
    Prefix  // Token must match the start of a target
}
```

Each token is assigned a `MatchScope` during query evaluation using:

```csharp
delegate MatchScope MatchScopeSelector(AutypoToken token, AutypoQueryContext queryContext);
```

You configure this using:

```csharp
.WithMatchScope(MatchScopeSelector selector)
```

---

### ✨ Convenience Methods

#### ✅ Default behavior (final token as prefix):

```csharp
.WithFinalTokenPrefixMatchScope()
```

This enables prefix matching only on the last token — ideal for autocomplete.

#### ❌ Disable prefix matching for final token:

```csharp
.WithFinalTokenFullMatchScope()
```

This forces tokens to match whole words — useful in strict search modes.

---

### 🧠 Dynamic Matching Based on Query Context

You can also define prefix behavior dynamically using query metadata:

```csharp
.WithMatchScope((token, context) =>
{
    bool isFinalQueryToken = token.Contains(context.QueryTokenizedLength - 1);

    if (isFinalQueryToken && context.Metadata.TryGetValue("mode", out var value) && value is "loose")
    {
        return MatchScope.Prefix;
    }

    return MatchScope.Full;
})
```

And at query time:

```csharp
complete.Complete("query", new AutypoSearchContext
{
    Metadata = new Dictionary<string, object>
    {
        { "mode", "loose" }
    }
});
```

---

## 🔄 Token Ordering

Token ordering determines whether query tokens must appear in the same sequence as the indexed document tokens.

By default, Autypo expects query tokens to match the document in order (though skipping is allowed).
You can override this to allow tokens to match in any order — useful for natural-language queries or search tags.

---

### 🔧 Defining Token Ordering

Ordering behavior is controlled via the `TokenOrdering` enum:

```csharp
enum TokenOrdering
{
    InOrder,        // Tokens must appear in the same sequence (default)
    StrictSequence, // Tokens must appear in the same sequence with no skipping
    Unordered       // Tokens may match in any order
}
```

> 📝 `InOrder` allows query tokens to appear in order but skips are allowed (e.g., "apple juice" matches "fresh apple organic juice").

> Use `StrictSequence` when the token sequence must match exactly with no intermediate tokens.


You control this with:

```csharp
delegate TokenOrdering TokenOrderingSelector(AutypoQueryContext queryContext);
```

And configure it like so:

```csharp
.WithTokenOrdering(TokenOrderingSelector selector)
```

---

### ✨ Convenience Methods

#### ✅ Enforce some order (default):

```csharp
.WithInOrderTokenOrdering()
```

Useful when user is expected to enter terms in order — and missing words are allowed.
For example, `"wireless mouse"` will match `"cheap wireless bluetooth mouse"`, but not `"mouse wireless"`.

---

#### 🔁 Allow any token order:

```csharp
.WithUnorderedTokenOrdering()
```

Matches query tokens regardless of their order in the document. Useful for search inputs like tags or keywords.

Recommended for fuzzy, tag-style, or natural language search.

#### ✅ Enforce strict order:

```csharp
.WithStrictSequenceTokenOrdering()
```

Useful when order affects semantics — such as legal terms or command-line flags.

---

## 🧩 Partial Token Matching

Autypo normally requires that all query tokens appear in a matching document. But for many real-world cases — such as tag-based search, autocomplete, or noisy input — allowing **partial matches** improves recall and user experience.

---

### 🔧 Defining a Partial Match Policy

Partial matching is controlled using the `PartialMatchPolicy` struct:

```csharp
public readonly struct PartialMatchPolicy
{
    public float Threshold { get; }

    public PartialMatchPolicy WithRequiredQueryToken(int tokenIndex);
}
```

- `Threshold`: The **fraction (0.0–1.0)** of non-required tokens that must be matched
- `WithRequiredQueryToken(...)`: Marks individual query tokens as **required** (must be matched)

This gives you control over how strict or lenient partial matching should be — down to individual query tokens.

You configure it with a delegate:

```csharp
delegate PartialMatchPolicy PartialTokenMatchingPolicySelector(
    ReadOnlySpan<AutypoToken> tokens,
    AutypoQueryContext queryContext);
```

And apply it using:

```csharp
.WithPartialTokenMatching(PartialTokenMatchingPolicySelector selector)
```

---

### ✨ Convenience Method

For basic usage, Autypo offers:

```csharp
.WithPartialTokenMatching()
```

This enables partial matching with default behavior:
- No tokens are marked as required
- At least one match is needed to consider the document valid

---

### ⚠️ Requires Unordered Token Matching

Partial token matching **only works** when the engine is also configured with unordered token ordering:

```csharp
.WithUnorderedTokenOrdering()
```

If you're using `WithOrderedTokenOrdering()` or `WithStrictSequenceTokenOrdering()`, Autypo will ignore partial token settings.

---

### 🧠 Practical Example

The following example shows how we can require the first query token to be present
in all results with the rest being optional. In this case, the remaining query tokens
will mainly contribute to ranking.

```csharp
.WithPartialTokenMatching((_, _) => PartialMatchPolicy
        .SomeQueryTokensRequired()
        .WithRequiredQueryToken(0))

```

---

## 🔠 Word Splitting & Joining

So far, we’ve seen how Autypo matches query tokens against document tokens. But what happens when users accidentally add or remove spaces?

- `hard ware` (query) vs. `hardware` (document)
- `NewYork` (query) vs. `New York` (document)

This is where tokenization and **bigram matching** come into play.

---

### 🧠 What is Bigram Matching?

Tokenization is the process of breaking text into individual tokens. Changing whitespace changes token boundaries, which affects matching.

Autypo can handle:
- **Split words**: `hard ware` vs. `hardware`
- **Joined words**: `NewYork` vs. `New York`

To do this, Autypo indexes and queries **word pairs** in addition to single tokens.

---

### 🔍 Example: Word Split

- `query`: `"hard ware"`
- `document`: `"hardware"`
- Autypo tokenizes query into `["hard", "ware", "hard ware"]`
- Autypo tokenizes document into `["hardware"]`
- With bigrams enabled on the **document**, Autypo checks if `"hardware"` matches any word *or* any 2-token sequence like `"hard"+"ware"`
- Autypo will add a space (`' '`) in between the joined words, which means a missing space counts as a typo.

---

### 🔍 Example: Word Join

- `query`: `"NewYork"`  
- `document`: `"New York"`  
- Autypo tokenizes query into `["NewYork"]`
- Autypo tokenizes document into `["New", "York", "New York"]`
- With bigrams enabled on the **query**, the document `"NewYork"` can still match.
- Autypo will add a space (`' '`) in between the joined words, which means an extra space counts as a typo.

---

### ⚙️ Enabling Word Split / Join Support

#### 🧩 To match when the **query has extra spaces** (e.g. `"hard ware"` vs. `"hardware"`):
```csharp
.WithTextAnalyzer(analyzer => analyzer
    .UseAlsoQueryTransformer(() => new NGramTokenTransformer(ngramLength: 2)))
```

#### 🧩 To match when the **query has missing spaces** (e.g. `"NewYork"` vs. `"New York"`):
```csharp
.WithTextAnalyzer(analyzer => analyzer
    .UseAlsoDocumentTransformer(() => new NGramTokenTransformer(ngramLength: 2)))
```

#### 🧩 To enable **both directions**:
```csharp
.WithTextAnalyzer(analyzer => analyzer
    .UseAlsoTransformer(() => new NGramTokenTransformer(ngramLength: 2)))
```

---

For a deeper dive into tokenization and analyzers, check out our [tokenization guide](./tokenizing.md).

---

## 🧪 Example: Very loose matching

The following example demonstrates how to configure autypo to search for names of products where:

- Query terms can be specified in any order
- Not all query terms must be found for a product to match
- 2 typos per word regardless of length
- The final term can be a prefix (default)
- Bigrams are used to detect if user has added or removed a space

```csharp
AutypoSearch<Product> search = await AutypoFactory.CreateSearchAsync<Product>(config => config
    .WithDataSource(() => ...)
    .WithIndex(p => p.Name, index => index
        .WithUnorderedTokenOrdering()
        .WithPartialTokenMatching()
        .WithFuzziness(maxEditDistance: 2)
        .WithFinalTokenPrefixMatchScope()
        .WithTextAnalyzer(analyzer => analyzer
            .UseAlsoTransformer(() => new NGramTokenTransformer(ngramLength: 2)))
    ));
```
