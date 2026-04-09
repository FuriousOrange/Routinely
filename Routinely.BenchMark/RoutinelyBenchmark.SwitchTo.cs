using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    [GlobalSetup(Target = nameof(Switch_To_Steady_State))]
    public void Switch_To_Steady_State_Setup()
    {
        static async Coroutine Producer()
        {
            while (steadyState)
            {
                await Coroutine.SwitchTo(Consumer);
            }
        }

        static async Coroutine Consumer()
        {
            while (steadyState)
            {
                await Coroutine.SwitchTo(Producer);
            }
        }

        for (var i = 0; i < Coroutines; i++)
        {
            Producer().Forget();
        }
    }

    [GlobalCleanup(Target = nameof(Switch_To_Steady_State))]
    public void Switch_To_Steady_State_Cleanup()
    {
        steadyState = false;
        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Switch_To_Steady_State()
    {
        Coroutine.ResumeAll();
    }

    internal class SteadyContext
    {
        public bool SteadyState;
    }

    private SteadyContext steadyContext = new SteadyContext();

    [GlobalSetup(Target = nameof(Switch_To_Context_Steady_State))]
    public void Switch_To_Context_Steady_State_Setup()
    {
        steadyContext = new SteadyContext
        {
            SteadyState = true
        };

        static async Coroutine Producer(SteadyContext steadyContext)
        {
            while (steadyContext.SteadyState)
            {
                await Coroutine.SwitchTo(steadyContext, static ctx => Consumer(ctx));
            }
        }

        static async Coroutine Consumer(SteadyContext steadyContext)
        {
            while (steadyContext.SteadyState)
            {
                await Coroutine.SwitchTo(steadyContext, static ctx => Producer(ctx));
            }
        }

        for (var i = 0; i < Coroutines; i++)
        {
            Producer(steadyContext).Forget();
        }
    }

    [GlobalCleanup(Target = nameof(Switch_To_Context_Steady_State))]
    public void Switch_To_Context_Steady_State_Cleanup()
    {
        steadyContext.SteadyState = false;

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Switch_To_Context_Steady_State()
    {
        Coroutine.ResumeAll();
    }
}
