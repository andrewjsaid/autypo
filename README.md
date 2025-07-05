# ⚡ Autypo

> Blazing-fast autocomplete and typo-tolerant search for .NET

Autypo is a powerful, developer-friendly search engine for **autocomplete**, **fuzzy matching**, and **short-string lookups**. Whether you're building a product search bar, form helper, or CLI — Autypo makes it effortless to deliver typo-tolerant, fast, and intelligent user experiences.

## 📚 Table of Contents

- [Getting Started](#-getting-started)
- [Installation](#-installation)
- [Features](#-features)
- [Limitations](#-limitations)
- [Perfect For](#-perfect-for)
- [Learning Resources](#-learning-resources)
- [Samples](#-samples)
- [Benchmarks](#-benchmarks)
- [Project Structure](#-project-structure)
- [Support and Feedback](#-support-and-feedback)
- [License](#-license)

## ✨ Getting Started

#### ✅ Basic Setup — In 30 Seconds

Autypo works out-of-the-box — no config required.
Just provide your data and start getting intelligent completions.

```csharp
using Autypo;
using Autypo.Configuration;

string[] movies = [
    "The Lord of the Rings: The Return of the King",
    "Star Wars: Return of the Jedi",
    "Batman Returns"
];

IAutypoComplete autypoComplete = await AutypoFactory.CreateCompleteAsync(config => config
    .WithDataSource(movies));

IEnumerable<string> suggestions = autypoComplete.Complete("retur of the");
// → "Star Wars: Return of the Jedi", "The Lord of the Rings: The Return of the King"

suggestions = autypoComplete.Complete("bamman rets");
// → "Batman Returns"
```

No tokenization setup. No fuzzy matching config. Just smart results.

#### ✅ ASP.NET Core Setup with Background Indexing

For real-world apps, Autypo integrates seamlessly with ASP.NET Core and supports background indexing, multi-field search, and dependency-injected data sources.

```csharp
using Autypo;
using Autypo.AspNetCore;
using Autypo.Configuration;

builder.Services.AddScoped<ProductsLoader>();

builder.Services.AddAutypoSearch<Product>(config => config
    .WithDataSource(serviceProvider => serviceProvider.GetRequiredService<ProductsLoader>())
    .WithBackgroundLoading(UninitializedBehavior.ReturnEmpty)

    .WithIndex(product => product.Name, index => index
        .WithAdditionalKeys(product => [product.Description]) // Index name + description
        .WithUnorderedTokenOrdering() // Order of words doesn't matter
        .WithPartialTokenMatching() // Partial queries still match
        // ... and many more configuration options!
    ));

```

Define a search endpoint using dependency-injected Autypo:

```csharp
app.MapGet("/products/search", async (
    [FromQuery] string query,
    [FromServices] IAutypoSearch<Product> search) =>
{
    var results = await search.SearchAsync(query);
    return results.Select(r => r.Value);
});
```


**Example Query:**

```
GET /products/search?query=fngerprint usb recogn
```

**Example Result:**

```json
[
 {
   "code": "A1067",
   "name": "Fingerprint USB",
   "description": "Secure USB flash drive with fingerprint encryption"
 },
]
```

---

## 📦 Installation

Autypo is available on NuGet.

For **ASP.NET Core** integration (includes the core engine):

```bash
dotnet add package Autypo.AspNetCore
```

For **standalone or non-web scenarios** (e.g., console apps, background jobs):

```bash
dotnet add package Autypo
```

---

## 💡 Features

Autypo gives you fast, typo-tolerant search with modern developer ergonomics — zero config to start, but extreme tuning if you need it.

### 🧠 Intelligent Matching

- Typo-tolerant by default — fuzzy, partial, and out-of-order input
- Token-level matching control (prefix, fuzziness)
- Opt-in partial matching and out-of-order input
- Built to rank “real world” input
- Multi-index support with priority-based short-circuiting

### 🔧 Deeply Configurable

- Index multiple fields (e.g. `name + description`)
- Plug in custom scorers, filters, and match policies
- Add tokenizers and analyzers (N-grams, stemming, etc)
- Per-query tuning for fuzziness, partial match, max results

### ⚙️ Production-Ready

- Blazing-fast in-memory autocomplete via `IAutypoComplete`
- Full-featured, multi-index search engine via `IAutypoSearch<T>`
- Eager, background, or lazy indexing — your choice  
- Refresh indexes at runtime with `AutypoRefreshToken`
- Async-first, thread-safe, allocation-conscious

### 🌐 Framework-Friendly

- ASP.NET Core integration with full DI support
- Blazor Server–ready (ideal for typeahead inputs)
- Fully .NET-native — no native bindings, no hosted servers

---

✅ Open source & MIT licensed  
🛡️ Built for scale and safety  
🎉 Developer-first design

---

## ⚠️ Limitations

Autypo is intentionally designed for **short-text and autocomplete scenarios** — not full-text search.

- Documents are limited to **64 tokens max** (e.g., product names, commands, short descriptions)
- In-memory indexing is fast and efficient, but may not suit massive corpora or multi-megabyte documents
- Autypo is not intended for long-form text search — for that, consider other tools.

> This tradeoff is deliberate: it keeps Autypo incredibly fast, predictable, and ideal for UX-critical search.

---

## 🎯 Perfect For

- E-commerce product & SKU search
- Admin panel filtering and form helpers
- Smart command palettes
- CLI "Did You Mean?" suggestions
- Blazor autocomplete inputs
- Live-search over reference data (countries, tags, part numbers)
- Fuzzy full-text matching over short documents

---

## 📚 Learning Resources

### 📖 Guides

| Guide | Description |
|-------|-------------|
| [ASP.NET Core Integration](docs/aspnet-integration.md) | End-to-end setup with dependency injection and hosted services |
| [Indexing & Data Loading](docs/indexing.md) | Data sources, refresh tokens |
| [Search & Matching](docs/searching.md) | Customize fuzzy logic, token ordering and more |
| [Text Analysis & Tokenization](docs/tokenizing.md) | Configure tokenizers, transformers, and analyzers |
| [Custom Scoring & Filtering](docs/scoring.md) | Enrich matches with metadata and business logic |


---

### 💻 Samples

| Sample                                                        | What it demonstrates                                                            |
|---------------------------------------------------------------|---------------------------------------------------------------------------------|
| ["Did You Mean?" Console Demo](samples/ConsoleDidYouMean) | **Interactive CLI Helper** – Suggests valid commands with fuzzy matching        |
| [ASP.NET Core Search API](samples/ApiSearchProducts)      | **Product Search** – REST API with background indexing and multi-field matching |
| [Blazor City Autocomplete](samples/BlazorCities)          | **Real-Time Search UI** – Typeahead with typo tolerance and reindexing          |



---

## 📊 Benchmarks

Autypo competes head-to-head with Lucene and outperforms many .NET libraries for short-string search:

| Library              | Multi-Token | Index Time | Search Time | Avg/Search |
|----------------------|-------------|------------|-------------|------------|
| **Autypo**           | ✅ Yes      | 163 ms     | 3.34 s      | 1.33 ms    |
| Lucene (Fuzzy)       | ✅ Yes      | 776 ms     | 2.91 s      | 1.16 ms    |
| Lucene (Suggest)     | ❌ No       | 1.12 s     | 503 ms      | 0.20 ms    |
| [Levenshtypo](https://github.com/andrewjsaid/levenshtypo)*        | ❌ No       | 25 ms      | 312 ms      | 0.12 ms    |
| FuzzyWuzzy           | ❌ No       | 1 ms       | 4m 49s      | 92.56 ms   |

\* [Levenshtypo](https://github.com/andrewjsaid/levenshtypo) is the fuzzy string search library that powers
Autypo’s low-level approximate matching engine. It supports typo-tolerant single-token search using
optimized Tries and Levenshtein Automata.

⚠️ [See full benchmark repo and disclaimer →](https://github.com/andrewjsaid/autypo-benchmarks)

---

## 📁 Project Structure

```
/src
  └── Autypo/                   # Core library
  └── Autypo.AspNetCore/        # ASP.NET Core integration
  └── Autypo.Benchmarks/        # Microbenchmarks
  └── Autypo.IntegrationTests/
  └── Autypo.UnitTests/
  └── Autypo.Aot/               # Ensures that Autypo works in AOT scenarios
  
/samples
  └── ApiSearchProducts/        # REST API example
  └── BlazorCities/             # Blazor Server autocomplete
  └── ConsoleDidYouMean/        # CLI demo
```

---

## 💬 Support and Feedback

🧠 Have a question?  
💡 Got feedback?  
🐛 Found a bug?

Open an [issue](https://github.com/andrewjsaid/autypo/issues) or start a [discussion](https://github.com/andrewjsaid/autypo/discussions). We'd love to hear from you.

---

## ⚖️ License

Autypo is released under the MIT License.

---

## 🙋‍♂️ What's in a Name?

**Auto**complete + **Typo**-tolerance = Autypo.

Simple name, serious power.

---

Built for modern .NET developers.
