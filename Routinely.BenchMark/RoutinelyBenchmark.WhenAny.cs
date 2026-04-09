using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    private Coroutine[] whenAnyWork;

    [GlobalSetup(Targets = [
        nameof(When_Any_Async),
        nameof(When_Any_Sync)])]
    public void When_Any_Setup()
    {
        whenAnyWork = new Coroutine[Coroutines];
    }

    [Benchmark]
    public void When_Any_Async()
    {
        static async Coroutine Work()
        {
            await Coroutine.Yield();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAnyWork[i] = Work();
        }

        Coroutine.WhenAny(whenAnyWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_Any_Sync()
    {
        static async Coroutine Work()
        {
            /* sync */
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAnyWork[i] = Work();
        }

        Coroutine.WhenAny(whenAnyWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    private Coroutine<int>[] whenAnyResultWork;

    [GlobalSetup(Targets = [
        nameof(When_Any_Result_Async),
        nameof(When_Any_Result_Sync)])]
    public void When_Any_Result_Setup()
    {
        whenAnyResultWork = new Coroutine<int>[Coroutines];
    }

    [Benchmark]
    public void When_Any_Result_Async()
    {
        static async Coroutine<int> Work()
        {
            await Coroutine.Yield();
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAnyResultWork[i] = Work();
        }

        Coroutine.WhenAny(whenAnyResultWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void When_Any_Result_Sync()
    {
        static async Coroutine<int> Work()
        {
            return 1;
        }

        for (int i = 0; i < Coroutines; i++)
        {
            whenAnyResultWork[i] = Work();
        }

        Coroutine.WhenAny(whenAnyResultWork).Forget();

        while (Coroutine.ResumeAll()) ;
    }
}