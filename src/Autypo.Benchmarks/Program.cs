using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, DefaultConfig.Instance
        .AddJob(BenchmarkDotNet.Jobs.Job.ShortRun)
        .AddDiagnoser(MemoryDiagnoser.Default));
