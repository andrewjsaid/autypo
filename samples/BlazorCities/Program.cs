using Autypo.AspNetCore;
using Autypo.Configuration;
using Autypo.Tokenization;
using BlazorCities;
using BlazorCities.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<CityDatabase>();
builder.Services.AddAutypoSearch<City>(config => config
    .WithDataSource(sp => sp.GetRequiredService<CityDatabase>().Cities)

    .WithIndex(city => city.Name, index => index
        .WithAdditionalKeys(city => [$"{city.Name} {city.Country}"]) // Sometimes users might suffix with the country name
        .AddNoMinimumLengthQueryFilter() // For demo purposes we want to show results at the first keypress
        .WithTextAnalyzer(analyser => analyser // Custom analyzer to include bigrams
            .UseAlsoTransformer(() => new NGramTokenTransformer(ngramLength: 2)) // NGram(N=2) is a bigram which will join words together
        )
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
