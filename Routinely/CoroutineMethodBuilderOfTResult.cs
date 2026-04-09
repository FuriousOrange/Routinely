namespace Routinely;

public struct CoroutineMethodBuilder<TResult> : ICoroutineMethodBuilder<Coroutine<TResult>, TResult>
{
    internal Coroutine<TResult> coroutine;

    public readonly Coroutine<TResult> Task
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => coroutine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CoroutineMethodBuilder() => coroutine = new Coroutine<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CoroutineMethodBuilder<TResult> Create() => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetException(Exception ex)
        => ICoroutineMethodBuilder<Coroutine<TResult>, TResult>.SetException(in coroutine, ex);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult(TResult result)
        => ICoroutineMethodBuilder<Coroutine<TResult>, TResult>.SetResult(ref coroutine, ref result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
        => awaiter.OnAwaiting(ref coroutine, ref awaiter, ref stateMachine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
        => AwaitOnCompleted(ref awaiter, ref stateMachine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
        => ICoroutineMethodBuilder<Coroutine<TResult>, TResult>.StartInternal(ref stateMachine);

    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        // No-op for struct-based builders with thread affinity
        // State machine is already captured in Start()
    }
}