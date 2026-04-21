using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    //[Benchmark]
    public void Cancel()
    {
        static async Coroutine Work()
        {
            await Coroutine.Yield();
        }

        for(var i = 0; i < Coroutines; i++)
        {
            var co = Work();
            co.Cancel();
        }

        while(Coroutine.ResumeAll()) ;
    }

    //[Benchmark]
    public void Cancel_Stack_Depth_5_Async()
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
            var co = Work(5);
            co.Cancel();    
        }

        while (Coroutine.ResumeAll()) ;
    }

    //[Benchmark]
    public void Cancel_Contract()
    {
        static async Coroutine Work()
        {
            using var ctc = await CancellationContract.Enter();

            var pooled = Exchange<object>.Reserve();

            try
            {
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
            }
            finally
            {
                pooled.Return();
            }
        }

        for (var i = 0; i < Coroutines; i++)
        {
            var co = Work();
            co.Cancel();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Cancel_Contract_Stack_Depth_5_Async()
    {
        static async Coroutine Work(int depth)
        {
            using var ctc = await CancellationContract.Enter();

            var pooled = Exchange<object>.Reserve();

            try
            {
                if (depth < 5)
                {
                    await Work(depth + 1).Enforce(ctc);
                }

                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);

                return;
            }
            finally
            {
                pooled.Return();
            }
        }

        for (int i = 0; i < Coroutines; i++)
        {
            var co = Work(5);
            co.Cancel();
        }

        while (Coroutine.ResumeAll()) ;
    }
}
