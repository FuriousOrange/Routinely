using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    private Coroutine[] whenAllWork;

    [GlobalSetup(Targets = [nameof(When_All_Async), nameof(When_All_Sync)])]
    public void When_All_Setup()
    {
        whenAllWork = new Coroutine[Coroutines];
    }

    [Benchmark]
    public void When_All_Async()
    {
        static async Coroutine Work()
        {
            await Coroutine.Yield();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_All_Sync()
    {
        static async Coroutine Work()
        {
            /* sync */
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    private Coroutine<int>[] whenAllResultWork;
    private int[] whenAllResults;

    [GlobalSetup(Targets =
        [nameof(When_All_Result_Async),
        nameof(When_All_Result_Sync),
        nameof(When_All_Result_Async_Non_Allocating),
        nameof(When_All_Result_Sync_Non_Allocating)
        ])]
    public void When_All_Result_Setup()
    {
        whenAllResultWork = new Coroutine<int>[Coroutines];
        whenAllResults = new int[Coroutines];
    }

    [Benchmark]
    public void When_All_Result_Async()
    {
        static async Coroutine<int> Work()
        {
            await Coroutine.Yield();
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllResultWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllResultWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_All_Result_Sync()
    {
        static async Coroutine<int> Work()
        {
            /* sync */
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllResultWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllResultWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_All_Result_Async_Non_Allocating()
    {
        static async Coroutine<int> Work()
        {
            await Coroutine.Yield();
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllResultWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllResultWork, whenAllResults).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_All_Result_Sync_Non_Allocating()
    {
        static async Coroutine<int> Work()
        {
            /* sync */
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAllResultWork[i] = Work();
        }

        Coroutine.WhenAll(whenAllResultWork, whenAllResults).Forget();

        while (Coroutine.ResumeAll()) ;
    }
}
