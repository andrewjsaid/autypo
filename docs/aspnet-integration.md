# 🌐 ASP.NET Core Integration Guide

Autypo integrates seamlessly with ASP.NET Core applications, enabling intelligent, fuzzy, multi-field search using dependency injection and background indexing.

This guide walks through configuring Autypo in an ASP.NET Core app.

---

## 🛠️ Installation

You only need to install the **ASP.NET Core integration package** — it includes the core `Autypo` library automatically:

```bash
dotnet add package Autypo.AspNetCore
```

This package gives you everything you need:

* The full Autypo engine
* ASP.NET Core integration hooks for dependency injection registrations

---

## Summary

- Register `AddAutypoComplete` and inject `IAutypoComplete` for string matching.
- Register `AddAutypoSearch<T>` and inject `IAutypoSearch<T>` for matching any `T`.
- Use the keyed alternatives (`AddKeyedAutypoComplete<T>("key", ...)` or `AddKeyedAutypoSearch<T>("key", ...)`) with `[FromKeyedServices("key")]` when you need multiple Autypo engines of the same type.
- All Autypo services are registered as singletons and are fully thread-safe for both search and reindex operations.
- Use `.WithDataSource(serviceProvider => ...)` to load your data using services resolved from the DI container.


## 🧩 Configuring Autypo Services

In your main registration logic, register Autypo for Completion or Search in the following snippets.

```csharp
using Autypo;
using Autypo.AspNetCore;
using Autypo.Configuration;
```

To register `IAutypoComplete` and `IAutypoRefresh` use:

```csharp
builder.Services.AddAutypoComplete(config => config
    .WithDataSource(["apples", "oranges"]));

// The next registration will make the same interfaces available if used with [FromKeyedServices("key")]
builder.Services.AddKeyedAutypoComplete("key", config => config
    .WithDataSource(["apples", "oranges"]));
```

To register `IAutypoSearch<T>` and `IAutypoRefresh<T>` use:

```csharp
builder.Services.AddAutypoSearch<Fruit>(config => config
    .WithDataSource([new Fruit("apples"), new Fruit("oranges")])
    .WithIndex(fruit => fruit.Name));

// The next registration will make the same interfaces available if used with [FromKeyedServices("key")]
builder.Services.AddKeyedAutypoSearch<Fruit>("key", config
    .WithDataSource([new Fruit("apples"), new Fruit("oranges")])
    .WithIndex(fruit => fruit.Name));
```

There's much more you can configure with Autypo — check out our other guides.

---

## 🔍 Defining a Search Endpoint

You can inject Autypo search into your controller or route like any other service:

```csharp
app.MapGet("/fruit/search", async (
    [FromQuery] string query,
    [FromServices] IAutypoComplete autypoComplete) =>
{
    return await autypoComplete.CompleteAsync(query);
});
```

---

## 🔄 Refreshing the Index

To refresh or reindex data dynamically (e.g. after a database update), inject `IAutypoRefresh` or `IAutypoRefresh<T>` into your endpoint:

```csharp
app.MapPost("/fruit/reload", async ([FromServices] IAutypoRefresh refresh) =>
{
    await refresh.RefreshAsync(); // Waits for reindex to complete
    return Results.Ok();
});
```

If you want to trigger the refresh in the background without blocking the request:

```csharp
app.MapPost("/fruit/reload", ([FromServices] IAutypoRefresh refresh) =>
{
    _ = Task.Run(() => refresh.RefreshAsync());
    return Results.Accepted();
});
```

✅ Reindexing is atomic and thread-safe. Existing queries continue using the old index until the new one is ready.

## 📊 Telemetry & Observability

Autypo is instrumented with [`System.Diagnostics.Activity`](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activity) to support OpenTelemetry-based tracing. This allows you to track search and indexing performance using your existing observability stack (e.g., Application Insights, Jaeger, Grafana, etc.).

To enable telemetry, subscribe to the `ActivitySource` named `Autypo`:

### 🔍 Search Tracing

Autypo will emit an activity named `Autypo.Search` when a query is executed.

Tags include:
- `autypo.query`: the raw query string
- `autypo.uninitialized`: present if the index wasn't yet loaded
- `autypo.results`: total number of results returned
- `autypo.hits`: number of actual matches before filtering

### 🔄 Indexing Tracing

When an index is refreshed or rebuilt, Autypo emits `Autypo.Reload`:

Tags include:
- `autypo.documents`: number of items processed during indexing

> These hooks are ideal for performance profiling, debugging slow queries, or integrating with distributed tracing systems.

## FAQs

### What’s the default indexing mode?

By default, Autypo uses eager indexing. You can change this using .WithBackgroundLoading() or .WithLazyLoading() during configuration.
Eager indexing happens on startup and may increase app boot time — useful when you want results to be immediately available.

### What happens if a query comes in before the background index finishes?

If you’re using background or lazy indexing, you must use CompleteAsync or SearchAsync,
which will transparently wait for the index to initialize. The synchronous versions
may return empty results or throw, depending on your configuration.

Reindexing is thread-safe and seamless. Ongoing searches will use the previous index
until the new one is fully loaded and atomically swapped in.


For more, check out the [indexing guide](indexing.md)
