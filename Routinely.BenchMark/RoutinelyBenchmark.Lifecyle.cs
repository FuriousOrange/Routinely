using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    [Benchmark(Baseline = true)]
    public void Single_Async()
    {
        static async Coroutine Work()
        {
            await Coroutine.Yield();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Single_Result_Async()
    {
        static async Coroutine<int> Work()
        {
            await Coroutine.Yield();
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Single_Sync()
    {
        static async Coroutine Work()
        {
            /* sync */
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }
    }

    [Benchmark]
    public void Single_Result_Sync()
    {
        static async Coroutine<int> Work()
        {
            /* sync */
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }
    }
}
