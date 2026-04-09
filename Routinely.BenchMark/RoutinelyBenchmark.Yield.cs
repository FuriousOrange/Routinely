using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    private static bool steadyState = true;

    [GlobalSetup(Target = nameof(Yield_Steady_State))]
    public void Yield_Steady_State_Setup()
    {
        async Coroutine Work()
        {
            while (steadyState)
            {
                await Coroutine.Yield();
            }
        }

        for (var i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        steadyState = true;
    }

    [GlobalCleanup(Target = nameof(Yield_Steady_State))]
    public void Yield_Steady_State_Cleanup()
    {
        steadyState = false;

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Yield_Steady_State()
    {
        Coroutine.ResumeAll();
    }
}
