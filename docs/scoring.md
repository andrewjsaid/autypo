# 📈 Scoring & Ranking

Autypo gives you full control over how matched candidates are **tagged, filtered, scored, and ranked**. This pipeline makes it easy to implement relevance tuning, personalization, dynamic thresholds, or business-specific weighting logic.

This guide covers:

- The matching pipeline: **Tag ➝ Filter ➝ Score**
- How to use `MaxResults(...)` to limit result count
- How the **default scorer** works (and how to customize it)

---

## 🧠 The Matching Pipeline

After Autypo finds candidates that meet the query’s token-level matching rules, it runs them through the **tag → filter → score** pipeline:

### 1. 🏷️ Tagging (`CandidateTagger<T>`)

Used to annotate candidates with business or match-specific metadata (via `.Tags`):

```csharp
delegate void CandidateTagger<T>(
    MatchCandidate<T> candidate,
    AutypoQueryContext queryContext);
```

Example:

```csharp
.WithCandidateTagger((candidate, ctx) =>
{
    if (candidate.GetExtractedDocumentTokenInfo(0).Token?.Text.Equals("DEFINE:") == true)
    {
        candidate.Tags.Set("is_define", true);
    }
})
```

Tags can be used later in scoring or filtering.

---

### 2. 🚫 Filtering (`CandidateFilter<T>`)

Used to **exclude** candidates from final ranking:

```csharp
delegate bool CandidateFilter<T>(
    MatchCandidate<T> candidate,
    AutypoQueryContext queryContext);
```

Example: skip results without at least 2 exact matches

```csharp
.WithCandidateFilter((c, _) => c.QueryExactMatchCount >= 2)
```

---

### 3. 🧮 Scoring (`CandidateScorer<T>`)

Used to compute a floating-point score for each candidate:

```csharp
delegate float CandidateScorer<T>(
    MatchCandidate<T> candidate,
    AutypoQueryContext queryContext);
```

This happens **after** tagging and filtering. All remaining results are then sorted by score (descending) and returned.

---

## 🎯 Limiting Results: `MaxResults(...)`

Autypo lets you dynamically limit the number of top-scoring results returned:

```csharp
delegate int? MaxResultsSelector(AutypoSearchContext searchContext);
```

For example to configure the limit dynamically per query:

```csharp
.WithMaxResults(context =>
    context.Metadata.TryGetValue("limit", out var v) && v is int i ? i : 10)
```

### ✨ Convenience Method

For a constant choice of max results, use:

```csharp
.WithMaxResults(10)
```

> ℹ️ Even if a document matches in multiple indices (e.g. name and description),
> it will only appear once in the final result set.

---

## 🧰 Default Scoring Explained

Autypo includes a robust default scoring algorithm based on a **weighted linear model**.

---

### Scoring Factors:

| Factor                     | Description                                           |
|----------------------------|-------------------------------------------------------|
| Document score             | Optional boost set at indexing time                  |
| Query coverage             | How many tokens matched (exact, near, fuzzy)         |
| Query sequential/in-order  | Are matches consecutive and/or aligned               |
| Query length fit           | Are the query and doc similar in length              |
| Query early match          | Did the match occur early in the query               |
| Final token suffix         | Useful for autocomplete                              |
| Document coverage          | Similar to query coverage, but from document's side  |
| Document structure         | Sequential / in-order matches in the document        |
| Document early match       | Like query early match, but on document tokens       |

---

### ⚙️ Configuration

The default scorer is highly tunable. You can configure the weight and influence of each scoring factor by customizing the scoring profile. For most users, the defaults work well — but advanced users can explore the source for deeper control.

---

### Advanced: Use Your Own Scorer

You can override the default completely, for example
by computing the similarity between the query and the
document.

```csharp
.WithCandidateScorer((candidate, ctx) => ComputeSimilarity(ctx.Query, candidate.Document))
```
