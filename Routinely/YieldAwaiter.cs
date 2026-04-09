namespace Routinely;

public struct YieldAwaiter : ICoroutineNotifyAwaiting
{
    [ThreadStatic]
    private static CoroutineStack stack;

    static YieldAwaiter()
    {
        stack = null!;
    }

    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public readonly CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => stack;
    }

    public ExchangeToken<CoroutineCore> CoreToken { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly YieldAwaiter GetAwaiter()
    {
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetResult()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly void ICoroutineNotifyAwaiting.OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
    {
        if (coroutine.IsCompleted)
        {
            // Request and dispatch a coroutine, capture the stack
            stack = StackDispatcher.GetStack();
            coroutine.Configure(ref stateMachine, stack);
        }
        else
        {
            // Otherwise just return the current stack we're working with
            stack = StackDispatcher.CurrentStack;
        }
    }
}
