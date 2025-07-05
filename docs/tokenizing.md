# Tokenizing

:warning: Tokenization is an Advanced Topic and is mostly relevant to power users.

Tokenizing is the process of converting text (query or document) into smaller chunks called tokens.
In the vast majority of applications a token is equivalent to a word, and thus Autypo defaults
to tokenization by splitting the input by whitespace.

:info: Punctuation marks, dashes, underscores are all kept when using the default `WhitespaceTokenizer`.

Once the input text is broken up into tokens, each token is sequentially assigned a unique index starting from 0.
Keeping this model of sequentially incrementing tokens is important to understand how Autypo works.

After tokenization comes token transformation. During token transformation, the tokens created during
tokenization are processed by any number of transformers, which can remove, replace, add or join tokens.

The following guide explores these concepts in more detail.

## Tokenization

Tokenization is driven by the following types:

```csharp
interface IAutypoTokenizer
{
    void Tokenize(ReadOnlyMemory<char> text, TokenSegmentConsumer consumer);
}

class TokenSegmentConsumer
{
    void Accept(AutypoTokenSegment tokenSegment) { ... }
}

struct AutypoTokenSegment(
    int leadingTrivia,
    int tokenizedLength,
    int trailingTrivia) { ... }
```

> Note: Some details have been omitted for brevity and clarity.

The Tokenizer is given the text to tokenize and splits it into either trivia or tokens.
By the end of Tokenization, every part of the text must be classified as either token
or trivia.

The tokens extracted during tokenization are called the `ExtractedTokens` and have an
`IsOriginal = true`.

Thus, `extractedTokens[n].SequenceStart == n` is an invariant, as is
`extractedTokens[n].SequenceLength == 1`.

## Token Transformation

Token Transformation is driven by the following types:

```csharp
interface IAutypoTokenTransformer
{
    void Transform(AutypoToken token, TokenConsumer consumer);
}

sealed class TokenConsumer
{
    void Accept(AutypoToken token) { ... }
}

class AutypoToken
{
    int SequenceStart { get; }
    int SequenceLength { get; }
    int SequenceEnd { get; }
    bool Contains(int sequenceNumber) { ... }
    bool IsOriginal { get; }
    bool IsDeleted { get; }
    ReadOnlyMemory<char> Text { get; }
}

```

> Note: Some details have been omitted for brevity and clarity.

Transformation may drop, create, or join the extracted tokens to influence
and direct matching. Transformation may not change the number or sequence
of the extracted tokens. The result of transformation is `transformedTokens`
which has no similar invariants to `extractedTokens`.

For example tokenization might remove a word, resulting in no token representing Sequence `1`.
In this case Autypo matching treats sequence `1` as a Skipped token which the matching logic
knows how to handle correctly. A practical example follows:

```
Document: "The bird is the word"
  Extracted Tokens: ["the", "bird", "is", "the", "word"]
  Transformed Tokens (stopword removal): ["bird", "word"]

Query "bird is word"
  Extracted Tokens: ["bird", "is", "word"]
  Transformed Tokens (stopword removal): ["bird", "word"]

Match: Sequentially :checkbox:
```

Tokenization might also add an extra token for the same sequence number - for example an alternative
spelling of a word. In this case **both** tokens can be matched, and the "strongest" match is used.
See the following section for an example:

```
Document: "Star Wars"
  Extracted Tokens: ["Star", "Wars"]
  Transformed Tokens: [0 => "Star", 1 => "Wars", 1 => "Trek"]

Query "Star Trek"
  Extracted Tokens: ["Star", "Trek"]
  Transformed Tokens: ["Star", "Trek"]

Match: Exact :checkbox:
```

:point-up: example chosen for maximum controversy.

Finally, tokenization might also add tokens which span multiple sequence numbers,
effectively joining words. See the Word Splitting & Joining section in
[Tokenizing guide](./tokenizing.md) for a detailed example.


## Configuration

Text Analysis (Tokenization + Token Transformation) is configured per-index.
This allows for maximum flexibility - for example you could also index the
same field twice, using different analyzers per index if that provides the
intended behavior.

To configure Text Analysis on an index, use:

```csharp
.WithTextAnalyzer(analyzer => ...);
```

The `analyzer` object allows configuration of document text and query text
either independently or together, using the following methods:

```csharp
.UseDocumentTokenizer(Func<IAutypoTokenizer> tokenizer)
.UseQueryTokenizer(Func<IAutypoTokenizer> tokenizer)
.UseTokenizer(Func<IAutypoTokenizer> tokenizer) // same as calling both the above

.UseDocumentTransformer(Func<IAutypoTokenTransformer> transformer)
.UseQueryTransformer(Func<IAutypoTokenTransformer> transformer)
.UseTransformer(Func<IAutypoTokenTransformer> transformer) // same as calling both the above
```

Adding multiple tokenizers will "pipe" the results of one into another.
For example one tokenizer might split text into sentences and the next
one will split those sentences into words, removing punctuation.

The same process is done for transformers - a transformer reads the
results of the previosu transformer and outputs new ones. Transformers
also support being called with `UseAlso...` (as below) which in addition
to outputting transformed tokens, output the input tokens too.

```csharp
.UseAlsoDocumentTransformer(Func<IAutypoTokenTransformer> transformer)
.UseAlsoQueryTransformer(Func<IAutypoTokenTransformer> transformer)
.UseAlsoTransformer(Func<IAutypoTokenTransformer> transformer) // same as calling both the above
```

