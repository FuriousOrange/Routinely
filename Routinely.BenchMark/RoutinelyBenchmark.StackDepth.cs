using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    [Benchmark]
    public void Stack_Depth_5_Async()
    {
        static async Coroutine Work(int depth)
        {
            if (depth < 5)
            {
                await Work(depth + 1);
            }

            await Coroutine.Yield();
            return;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work(5).Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Stack_Depth_5_Sync()
    {
        static async Coroutine Work(int depth)
        {
            if (depth < 5)
            {
                await Work(depth + 1);
            }

            return;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work(5).Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Stack_Depth_5_Result_Async()
    {
        static async Coroutine<bool> Work(int depth)
        {
            if (depth < 5)
            {
                await Work(depth + 1);
            }

            await Coroutine.Yield();
            return false;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work(5).Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Stack_Depth_5_Result_Sync()
    {
        static async Coroutine<bool> Work(int depth)
        {
            if (depth < 5)
            {
                await Work(depth + 1);
            }

            return false;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work(5).Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }
}
