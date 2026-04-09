namespace Routinely.UnitTests;

using Routinely;

public abstract class CoroutineTestBase
{
    [TestCleanup]
    public void DispatcherCleanup()
    {
        // Drain any coroutines that may still be pending, or have faulted stacks
        // to ensure a clean state for the next test.
        DrainCoroutines();
        DrainContexts();
    }

    public static void DrainCoroutines(bool @throw = false)
    {
        while (StackDispatcher.StackCount > 0)
        {
            {
                try
                {
                    while (Coroutine.ResumeAll())
                    {
                    }
                }
                catch (Exception)
                {
                    if (!@throw)
                        continue;

                    throw;
                }
            }
        }
    }

    public static void DrainContexts()
    {
        foreach(var context in StackDispatcher.Contexts)
        {
            Coroutine.SetContext(context);
            DrainCoroutines();
        }

        Coroutine.DisposeContexts();
    }
}