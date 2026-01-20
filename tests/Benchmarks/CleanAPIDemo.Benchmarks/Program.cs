using BenchmarkDotNet.Running;
using CleanAPIDemo.Benchmarks.Config;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, new BenchmarkConfig());
