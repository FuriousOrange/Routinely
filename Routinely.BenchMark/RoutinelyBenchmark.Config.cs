using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

[MarkdownExporter]
[MemoryDiagnoser]
public partial class RoutinelyBenchmark
{
    [Params(1,1000, 10_000, 100_000)]
    public int Coroutines { get; set; }
}