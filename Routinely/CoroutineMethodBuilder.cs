namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct CoroutineMethodBuilder() : ICoroutineMethodBuilder<Coroutine, Coroutine.CoroutineVoid>
{
    internal Coroutine coroutine = new();

    public readonly Coroutine Task
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => coroutine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CoroutineMethodBuilder Create() => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetException(Exception ex)
        => ICoroutineMethodBuilder<Coroutine, Coroutine.CoroutineVoid>.SetException(in coroutine, ex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetResult() => ICoroutineMethodBuilder<Coroutine, Coroutine.CoroutineVoid>.SetResultVoid(coroutine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
        => awaiter.OnAwaiting(ref coroutine, ref awaiter, ref stateMachine);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
    {
        AwaitOnCompleted(ref awaiter, ref stateMachine);
   }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
        => ICoroutineMethodBuilder<Coroutine, Coroutine.CoroutineVoid>.StartInternal(ref stateMachine);

    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        // No-op for struct-based builders with thread affinity
        // State machine is already captured in Start()
    }
}