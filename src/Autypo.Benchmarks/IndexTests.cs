using Autypo.Configuration;
using BenchmarkDotNet.Attributes;

namespace Autypo.Benchmarks;

public class SearchTests
{
    private string[] _items = default!;
    private IAutypoComplete _autypoComplete = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        _items = GenerateUtils.GenerateGuids(Count);
        _autypoComplete = await AutypoFactory.CreateCompleteAsync(config => config.WithDataSource(_items));
    }

    [Params(10, 100, 1000, 10_000, 100_000)]
    public int Count { get; set; }

    [Benchmark]
    public string Search() => _autypoComplete.Complete(_items[^1]).First();
}
