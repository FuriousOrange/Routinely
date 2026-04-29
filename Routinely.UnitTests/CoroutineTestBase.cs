namespace Routinely.UnitTests;

using Routinely;
using System.Collections.Concurrent;

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
        foreach (var context in StackDispatcher.Contexts)
        {
            Coroutine.SetContext(context);
            DrainCoroutines();
        }

        Coroutine.DisposeContexts();
    }
}

public abstract class CoroutineThreadTestBase : CoroutineTestBase
{
    private Thread thread;

    private ManualResetEventSlim startEvent;
    private ManualResetEventSlim stopEvent;

    private ConcurrentQueue<Action> actionQueue;
    private ConcurrentQueue<Exception> exceptionQueue;

    [TestInitialize]
    public void TestInitialize()
    {
        Coroutine.ThreadInit();

        thread = new Thread(Worker);
        startEvent = new ManualResetEventSlim();
        stopEvent = new ManualResetEventSlim();
        actionQueue = new ConcurrentQueue<Action>();
        exceptionQueue = new ConcurrentQueue<Exception>();
    }

    [TestCleanup]
    public void ThreadCleanup()
    {
        startEvent.Dispose();
        stopEvent.Dispose();
    }

    public void StartThread()
    {
        thread.Start();

        startEvent.Set();

        stopEvent.WaitHandle.WaitOne();

        var exceptions = exceptionQueue.ToList();

        if(exceptions.Count == 1)
        {
            throw exceptions[0];
        }
        else if(exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }
    }

    public void AddWork(Action action) => actionQueue.Enqueue(action);

    private void Worker()
    {
        Coroutine.ThreadInit();

        startEvent.Wait();

        try
        {
            while (actionQueue.TryDequeue(out var action))
            {
                action();
            }

            while (Coroutine.ResumeAll()) ;
        }
        catch(Exception ex)
        {
            exceptionQueue.Enqueue(ex);
        }
        finally
        {
            stopEvent.Set();

        }

        DrainCoroutines();
        DrainContexts();
    }
}