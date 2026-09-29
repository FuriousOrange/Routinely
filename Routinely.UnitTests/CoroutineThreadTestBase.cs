namespace Routinely.UnitTests;

using Routinely;
using Routinely.Policies;
using System.Collections.Concurrent;

public abstract class CoroutineThreadTestBase<TThreadPolicy> : CoroutineTestBase
    where TThreadPolicy: IThreadPolicy
{
    private Thread thread;

    private ManualResetEventSlim startEvent;
    private ManualResetEventSlim stopEvent;

    private ConcurrentQueue<Action> actionQueue;
    private ConcurrentQueue<Exception> exceptionQueue;

    [TestInitialize]
    public void TestInitialize()
    {
        Coroutine.SetThreadPolicy<TThreadPolicy>();
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

        stopEvent.WaitHandle.WaitOne(5000);

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