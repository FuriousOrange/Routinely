namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct ContextAwaiter(CoroutineContext context) : ICoroutineNotifyAwaiting
{
    private readonly CoroutineContext context = context;

    public CoroutineStack Stack => null!;

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => context == StackDispatcher.CurrentContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContextAwaiter GetAwaiter() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetResult()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TCoroutine : struct, ICoroutine
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
    {
        if (IsCompleted)
        {
            coroutine.CoreToken.Item.StateMachine.MoveNext();
        }
        else
        {
            if (coroutine.IsCompleted)
            {
                // New coroutine so we can configure it on new stack in the required context
                coroutine.Configure(ref stateMachine, StackDispatcher.GetStack());
            }

            StackDispatcher.CurrentContext!.MigrationQueue.Enqueue(coroutine.Stack, context);
        }
    }
}