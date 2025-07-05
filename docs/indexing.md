# 📦 Indexing in Autypo

Indexing is at the heart of how Autypo works. Whether you're building autocomplete or fuzzy object search, indexing controls how and when your data becomes searchable.

This guide covers:

- How to supply data using `WithDataSource(...)`
- Index lifecycle modes (eager, background, lazy)
- What happens when the index isn't ready
- How to trigger re-indexing

---

## 🗃️ Supplying Data with `WithDataSource(...)`

All Autypo engines need a data source to index. You configure this via `.WithDataSource(...)`, which accepts:

### ✅ Static or Inline Data

- A static in-memory collection:
  ```csharp
  .WithDataSource(["apple", "orange", "grape"])
  ```

- A function that returns data:
  ```csharp
  .WithDataSource(() => LoadMyData())
  ```

- An async factory:
  ```csharp
  .WithDataSource(async () => await GetFruitNamesAsync())
  ```

---

### 🧩 Using Dependency Injection (ASP.NET Core)

The `Autypo.AspNetCore` package adds DI-aware overloads for `WithDataSource(...)`, letting you resolve services at runtime:

- Using `IServiceProvider` synchronously:
  ```csharp
  .WithDataSource((IServiceProvider sp) => ...)
  ```

- Using `IServiceProvider` asynchronously:
  ```csharp
  .WithDataSource(async (IServiceProvider sp) => await ...)
  ```

🔄 These delegates run within a scoped IServiceProvider — ideal for database or per-request data access scenarios.

---

### 📦 Supplying `IAutypoDataSource<T>`

You can also implement and provide a reusable data source via `IAutypoDataSource<T>`.

```csharp
/// <summary>
/// Defines a contract for providing documents to an Autypo search engine instance.
/// </summary>
/// <typeparam name="T">The type of documents being indexed.</typeparam>
public interface IAutypoDataSource<T>
{
    /// <summary>
    /// Asynchronously loads documents to be indexed by Autypo.
    /// </summary>
    Task<IEnumerable<T>> LoadDocumentsAsync(CancellationToken cancellationToken);
}
```

---

## ⏳ Indexing Modes

By default, Autypo uses **eager loading** — the index is built as soon as you create the engine. You can change this behavior to defer indexing with:

### 🚀 Eager Loading (default)

```csharp
.WithEagerLoading()
```

- Index is built immediately after configuration
- `AutypoFactory.Create*Async(...)` waits for indexing to complete before returning
- Provides the fastest first-query performance
- ⚠️ May delay app startup in ASP.NET Core — use `WithBackgroundLoading()` to defer indexing if needed

---

### 🔄 Background Loading

```csharp
.WithBackgroundLoading(UninitializedBehavior.ReturnEmpty)
```

- Indexing begins immediately after configuration, on a background thread
- Early queries may return nothing until indexing completes
- Use `CompleteAsync` or `SearchAsync` to safely await initialization
- You can use `UninitializedBehavior.Throw` to raise an `InvalidOperationException` if the index isn’t ready

---

### 💤 Lazy Loading

```csharp
.WithLazyLoading()
```

- Index builds only when a query is made
- Requires `CompleteAsync` or `SearchAsync` to initialize the index
- Ideal for rarely-used or optional search engines

---

## 🔁 Rebuilding the Index

Autypo supports index reloading at runtime using refresh tokens. You can associate one or more engines with a shared `AutypoRefreshToken`, then trigger a rebuild whenever needed.

### 🛠️ Manual Refresh via Token

```csharp
using Autypo;
using Autypo.Configuration;

var token = new AutypoRefreshToken();

IAutypoComplete complete = await AutypoFactory.CreateCompleteAsync(config => config
    .WithDataSource(["apple", "orange"])
    .UseRefreshToken(token));
```

To rebuild all engines associated with this token:

```csharp
await token.RefreshAsync(); // Rebuilds the index with updated data
```

---

### 🧩 Using Dependency Injection (ASP.NET Core)

If you're using the ASP.NET Core integration (`Autypo.AspNetCore`), refresh handles are automatically registered in DI:

- `IAutypoRefresh` for `IAutypoComplete` (can be keyed or unkeyed)
- `IAutypoRefresh<T>` for `IAutypoSearch<T>` (also keyed or unkeyed)

Example endpoint to trigger a background refresh:

```csharp
using Autypo.AspNetCore;

app.MapPost("/fruit/reload", ([FromServices] IAutypoRefresh refresh) =>
{
    _ = Task.Run(() => refresh.RefreshAsync());
    return Results.Ok();
});
```

> 🔐 Refreshing is atomic and thread-safe. Ongoing queries continue to use the existing index until the new one is fully loaded and swapped in.


---

Check out the [Telemetry & Observability](aspnet-integration.md#-telemetry--observability) section for details on measuring index load times.

