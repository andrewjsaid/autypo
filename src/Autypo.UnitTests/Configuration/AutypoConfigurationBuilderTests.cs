using Autypo.Configuration;
using Shouldly;

namespace Autypo.UnitTests.Configuration;

public class AutypoConfigurationBuilderTests
{
    [Fact]
    public void When_all_defaults()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource(["the only required field is datasource"]);

        var built = builder.Build(collectionKey: "my key");
        var config = built.Config;

        config.CollectionKey.ShouldBe("my key");
        config.CollectionType.ShouldBe(typeof(string));
        config.MaxResultsSelector(new()).ShouldBe(10);
        config.KeepTokenization.ShouldBeFalse();
        config.InitializationMode.ShouldBe(InitializationMode.Eager);
        config.UninitializedDataSourceHandler.ShouldBeNull();
        config.Indices.ShouldHaveSingleItem();
        config.Indices[0].KeySelector("hi").ShouldBe("hi");
        config.ShouldIndex.ShouldBeNull();
        config.DocumentScorer.ShouldBeNull();
        config.EmptyQueryHandler(queryContext: null!).ShouldBeEmpty();

        built.RefreshTokens.ShouldBeEmpty();
    }

    [Fact]
    public async Task When_data_source_specified_as_factory_with_context()
    {
        var builder = new AutypoConfigurationBuilder<string>();

        builder.WithDataSourceFactory((context) => new TestStringDataSource([context?.ToString() ?? string.Empty]));
        builder.WithDataSourceContext("hello");

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        var documents = await config.DataSourceFactory(config.DataSourceContext).LoadDocumentsAsync(CancellationToken.None);
        documents.ShouldBe(["hello"]);
    }

    [Fact]
    public async Task When_data_source_specified_explicitly()
    {
        var builder = new AutypoConfigurationBuilder<string>();

        builder.WithDataSource(new TestStringDataSource(["a", "b", "c"]));

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        var documents = await config.DataSourceFactory(null).LoadDocumentsAsync(CancellationToken.None);

        documents.ShouldBe(["a", "b", "c"]);
    }

    [Fact]
    public async Task When_data_source_specified_as_array()
    {
        var builder = new AutypoConfigurationBuilder<string>();

        builder.WithDataSource(["1", "2", "3"]);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        var documents = await config.DataSourceFactory(null).LoadDocumentsAsync(CancellationToken.None);

        documents.ShouldBe(["1", "2", "3"]);
    }

    [Fact]
    public async Task When_data_source_specified_as_func()
    {
        var builder = new AutypoConfigurationBuilder<string>();

        builder.WithDataSource(() => ["1", "2", "3"]);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        var documents = await config.DataSourceFactory(null).LoadDocumentsAsync(CancellationToken.None);

        documents.ShouldBe(["1", "2", "3"]);
    }

    [Fact]
    public async Task When_data_source_specified_as_func_2()
    {
        var builder = new AutypoConfigurationBuilder<string>();

        builder.WithDataSource(_ => Task.FromResult<IEnumerable<string>>(["1", "2", "3"]));

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        var documents = await config.DataSourceFactory(null).LoadDocumentsAsync(CancellationToken.None);

        documents.ShouldBe(["1", "2", "3"]);
    }

    [Fact]
    public void When_initialization_mode_returns_empty()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithInitializationMode(InitializationMode.Lazy, UninitializedBehavior.ReturnEmpty);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.InitializationMode.ShouldBe(InitializationMode.Lazy);

        config.UninitializedDataSourceHandler.ShouldNotBeNull();
        var results = config.UninitializedDataSourceHandler("test", new());
        results.ShouldBeEmpty();
    }

    [Fact]
    public void When_initialization_mode_throws()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithInitializationMode(InitializationMode.Background, UninitializedBehavior.Throw);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.InitializationMode.ShouldBe(InitializationMode.Background);

        config.UninitializedDataSourceHandler.ShouldNotBeNull();
        Assert.Throws<InvalidOperationException>(() => config.UninitializedDataSourceHandler("test", new()));
    }

    [Fact]
    public void When_initialization_mode_eager()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithEagerLoading();

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.InitializationMode.ShouldBe(InitializationMode.Eager);
        config.UninitializedDataSourceHandler.ShouldBeNull();
    }

    [Fact]
    public void When_initialization_mode_lazy()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithLazyLoading(UninitializedBehavior.ReturnEmpty);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.InitializationMode.ShouldBe(InitializationMode.Lazy);
        config.UninitializedDataSourceHandler!("test", new()).ShouldBeEmpty();
    }

    [Fact]
    public void When_initialization_mode_lazy_then_uninitialization_handler_must_be_specified()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        Assert.Throws<ArgumentException>(() => builder.WithLazyLoading(UninitializedBehavior.None));
    }

    [Fact]
    public void When_initialization_mode_background()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithBackgroundLoading(UninitializedBehavior.ReturnEmpty);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.InitializationMode.ShouldBe(InitializationMode.Background);
        config.UninitializedDataSourceHandler!("test", new()).ShouldBeEmpty();
    }

    [Fact]
    public void When_initialization_mode_background_then_uninitialization_handler_must_be_specified()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        Assert.Throws<ArgumentException>(() => builder.WithBackgroundLoading(UninitializedBehavior.None));
    }

    [Fact]
    public void When_not_string_then_index_is_required()
    {
        var builder = new AutypoConfigurationBuilder<TestItem>();
        builder.WithDataSource([]);

        Assert.Throws<AutypoConfigurationException>(() => builder.Build(collectionKey: string.Empty));

        builder.WithIndex(x => x.Item1, _ => { });

        // Does not throw
        builder.Build(collectionKey: string.Empty);
    }

    [Fact]
    public void When_should_index_is_specified()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithShouldIndex(x => x[0] == 'r');

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.ShouldIndex.ShouldNotBeNull();
        config.ShouldIndex("required").ShouldBeTrue();
        config.ShouldIndex("zebra").ShouldBeFalse();
    }

    [Fact]
    public void When_document_scorer_is_specified()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithDocumentScorer(d => d.Length);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.DocumentScorer.ShouldNotBeNull();
        config.DocumentScorer("1").ShouldBe(1);
        config.DocumentScorer("22").ShouldBe(2);
        config.DocumentScorer("333").ShouldBe(3);
    }

    [Fact]
    public void When_max_results_is_specified_as_func_and_returns_number()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithMaxResults(_ => 2);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(2);
    }

    [Fact]
    public void When_max_results_is_specified_as_func_and_returns_null()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithMaxResults(_ => null);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(null);
    }

    [Fact]
    public void When_max_results_is_specified_as_number()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithMaxResults(25);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(25);
    }

    [Fact]
    public void When_max_results_is_specified_twice_then_last_one_is_chosen()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithMaxResults(25);
        builder.WithMaxResults(_ => 22);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(22);
    }

    [Fact]
    public void When_max_results_is_specified_as_single_result()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithSingleResult();

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(1);
    }

    [Fact]
    public void When_max_results_is_specified_as_unlimited()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithUnlimitedResults();

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.MaxResultsSelector(new()).ShouldBe(null);
    }

    [Fact]
    public void When_multiple_indexes()
    {
        var builder = new AutypoConfigurationBuilder<TestItem>();
        builder.WithDataSource([]);

        builder.WithIndex(x => x.Item1, _ => { });
        builder.WithIndex(x => x.Item2, _ => { });

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.Indices.Count.ShouldBe(2);
        config.Indices[0].KeySelector(new("1", "2")).ShouldBe("1");
        config.Indices[1].KeySelector(new("1", "2")).ShouldBe("2");
    }

    [Fact]
    public void When_enriched_metadata()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithEnrichedMetadata(AutypoMetadataEnrichment.IncludeDocumentTokenText);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.KeepTokenization.ShouldBeTrue();
    }

    [Fact]
    public void When_empty_query_handling_specified()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        builder.WithEmptyQueryHandling(_ => ["hello", "world"]);

        var built = builder.Build(collectionKey: string.Empty);
        var config = built.Config;

        config.EmptyQueryHandler(queryContext: null!).ShouldBe(["hello", "world"]);
    }

    [Fact]
    public void When_refresh_token_is_added()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        var token = new AutypoRefreshToken();

        builder.UseRefreshToken(token);

        var built = builder.Build(collectionKey: string.Empty);

        built.RefreshTokens.ShouldBe([token]);
    }

    [Fact]
    public void When_many_refresh_tokens_are_added()
    {
        var builder = new AutypoConfigurationBuilder<string>();
        builder.WithDataSource([]);

        var token1 = new AutypoRefreshToken();
        var token2 = new AutypoRefreshToken();

        builder.UseRefreshToken(token1);
        builder.UseRefreshToken(token2);

        var built = builder.Build(collectionKey: string.Empty);

        built.RefreshTokens.ShouldBe([token1, token2]);
    }

    private class TestStringDataSource(string[] entries) : IAutypoDataSource<string>
    {
        public Task<IEnumerable<string>> LoadDocumentsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(entries);
        }
    }

    private record TestItem(string Item1, string Item2);
}
