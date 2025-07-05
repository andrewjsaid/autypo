using Autypo;
using Autypo.AspNetCore;
using Autypo.Configuration;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabase, Database>(); // injected services can be scoped
builder.Services.AddScoped<ProductsLoader>();

builder.Services.AddAutypoSearch<Product>(config => config
    .WithDataSource(serviceProvider => serviceProvider.GetRequiredService<ProductsLoader>())
    .WithBackgroundLoading(UninitializedBehavior.ReturnEmpty)

    // The first index is by code.
    .WithIndex(product => product.Code, index => index
        .WithPriority(1) // High priority - if we find a match by code, don't bother searching by name/description
        .AddQueryFilter((s, _) => s.Length == 5 && s[0] is 'A' or 'a') // searches for a code must be exactly 5 chars and start with 'A'
        .WithNoFuzziness() // Exact matches only
    )
    
    // The next index (Name / Description) isn't as important
    .WithIndex(product => product.Name, index => index
        .WithPriority(0) // Explicitly make this a lower priority than code
        .WithAdditionalKeys(product => [product.Description]) // Also index description alongside name
        .WithUnorderedTokenOrdering() // user does not need to enter query terms in the order they appear on the product
        .WithPartialTokenMatching() // user can enter some query terms that are not present in the product
    ));

var app = builder.Build();

app.MapGet("/products/search", async ([FromQuery] string query, [FromServices] IAutypoSearch<Product> search) =>
{
    IEnumerable<AutypoSearchResult<Product>> searchResults = await search.SearchAsync(query);
    return searchResults.Select(result => result.Value);
});

app.Run();


public record Product(string Code, string Name, string Description);

public class ProductsLoader : IAutypoDataSource<Product>
{
    private readonly IDatabase _database;

    public ProductsLoader(IDatabase database)
    {
        // database is injected from DI
        _database = database;
    }

    public async Task<IEnumerable<Product>> LoadDocumentsAsync(CancellationToken cancellationToken) => await _database.GetProductsAsync();
}

// Simulate a database which can be injected into other services
public interface IDatabase
{
    Task<IEnumerable<Product>> GetProductsAsync();
}
