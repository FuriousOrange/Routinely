using BenchmarkDotNet.Attributes;

namespace Routinely.BenchMark;

public partial class RoutinelyBenchmark
{
    [Benchmark]
    public void Nested_Awaits_Async()
    {
        static async Coroutine Last()
        {
            await Coroutine.Yield();
        }

        static async Coroutine Middle()
        {
            await Last();
            await Last();
        }

        static async Coroutine First()
        {
            await Middle();
            await Middle();
        }

        static async Coroutine Work()
        {
            await First();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Nested_Awaits_Sync()
    {
        static async Coroutine Last()
        {
            /* sync */
        }

        static async Coroutine Middle()
        {
            await Last();
            await Last();
        }

        static async Coroutine First()
        {
            await Middle();
            await Middle();
        }

        static async Coroutine Work()
        {
            await First();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Nested_Awaits_Result_Async()
    {
        static async Coroutine<long> Last()
        {
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine<long> Middle()
        {
            var x = await Last();
            var y = await Last();
            return x + y;
        }

        static async Coroutine<long> First()
        {
            var x = await Middle();
            var y = await Middle();
            return x + y;
        }

        static async Coroutine<long> Work()
        {
            return await First();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }

    [Benchmark]
    public void Nested_Awaits_Result_Sync()
    {
        static async Coroutine<long> Last()
        {
            return 1;
        }

        static async Coroutine<long> Middle()
        {
            var x = await Last();
            var y = await Last();
            return x + y;
        }

        static async Coroutine<long> First()
        {
            var x = await Middle();
            var y = await Middle();
            return x + y;
        }

        static async Coroutine<long> Work()
        {
            return await First();
        }

        for (int i = 0; i < Coroutines; i++)
        {
            Work().Forget();
        }

        while (Coroutine.ResumeAll()) ;
    }
}
