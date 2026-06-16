namespace Routinely;

public interface ICoroutineNotifyAwaiting : INotifyCompletion
{
    CoroutineStack Stack { get; }

    bool IsCompleted { get; }

    void OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TCoroutine : struct, ICoroutine
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine;

    void INotifyCompletion.OnCompleted(Action continuation)
    {
        throw new InvalidOperationException();
    }

    void GetResult();
}

public interface ICoroutineNotifyAwaited<T> : ICoroutineNotifyAwaiting
{
    new T GetResult();
}
