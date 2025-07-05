using Autypo.Configuration;
using BenchmarkDotNet.Attributes;

namespace Autypo.Benchmarks;

public class IndexTests
{
    private string[] _items = [];

    [GlobalSetup]
    public void Setup()
    {
        _items = GenerateUtils.GenerateGuids(Count);
    }

    [Params(10, 100, 1000, 10_000, 100_000)]
    public int Count { get; set; }

    [Benchmark]
    public async Task<object> Index()
    {
        return await AutypoFactory.CreateCompleteAsync(config => config.WithDataSource(_items));
    }
}
