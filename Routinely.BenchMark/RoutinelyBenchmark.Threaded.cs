using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

[MemoryDiagnoser]
public class RoutinelyBenchmarkThreaded
{
    internal class WorkerContext(int coroutines, int workers)
    {
        private RoutinelyBenchmark benchmark;

        private Thread[] threads;

        private Barrier barrier;

        private volatile bool running = false;

        public void Setup()
        {
            benchmark = new RoutinelyBenchmark();

            barrier = new Barrier(workers + 1);

            threads = [.. Enumerable.Range(0, workers).Select(_ => new Thread(Work))];

            benchmark.Coroutines = coroutines / workers;

            running = true;

            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public void Work()
        {
            Coroutine.ThreadInit();

            while (running)
            {
                barrier.SignalAndWait();

                benchmark.Single_Async();

                barrier.SignalAndWait();
            }
        }

        public void Cleanup()
        {
            running = false;   
        }

        public void Start()
        {
            barrier.SignalAndWait();
        }

        public void Stop()
        {
            barrier.SignalAndWait();
        }
    }

    private WorkerContext context;

    [Params(1000_000)]
    public int Coroutines { get; set; }

    [Params(4)]
    public int Workers { get; set; }

    [GlobalSetup(Target = nameof(Threaded_Single_Async))]
    public void Threaded_Setup()
    {
        context = new WorkerContext(Coroutines, Workers);
        context.Setup();
    }

    public void Threaded_Cleanup()
    {
        context.Cleanup();
    }

    [Benchmark]
    [WarmupCount(10)]
    public void Threaded_Single_Async()
    {
        context.Start();
        context.Stop();
    }
}
