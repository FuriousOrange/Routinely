namespace Routinely.UnitTests;

public static class CoroutineTestExtensions
{
    public static void ResumeUntilCompletion(this ICoroutine coroutine, int maxExecutions = 500000)
    {
        var currentExecution = 0;

        while (Coroutine.Count > 0)
        {
            if (currentExecution >= maxExecutions)
                Assert.Fail($"The test failed after {maxExecutions} executions as the coroutine under test did not complete.");

            Coroutine.ResumeAll();

            currentExecution++;
        }
    }

    public static void ResumeUntil<T>(this T coroutine, Func<ICoroutine, bool> predicate)
        where T : ICoroutine
    {
        while (!predicate(coroutine))
        {
            Coroutine.ResumeAll();
        }
    }
}