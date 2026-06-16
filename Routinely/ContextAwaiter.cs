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
        if (context.DispatcherId != StackDispatcher.Id)
        {
            throw new InvalidOperationException("Cannot await context from a different dispatcher");
        }

        if (IsCompleted)
        {
            coroutine.CoreToken.Item.StateMachine.MoveNext();
        }
        else
        {
            CoroutineStack stack;

            if (coroutine.IsCompleted)
            {
                coroutine.Configure(ref stateMachine, StackDispatcher.GetStack());
                stack = coroutine.Stack;
            }
            else
            {
                stack = StackDispatcher.CurrentStack;
            }

            // Push a context state machine onto the stack, it will migrate on the next ResumeAll
            var coreToken = StackDispatcher.GetCoroutineCoreToken();
            var contextStateMachine = new ContextStateMachine(context, coreToken);
            ref var core = ref coreToken.Item;
            core.SetFlag(CoroutineCore.Awaited);
            core.StateMachine = CoroutineStateMachine<ContextStateMachine>.CaptureStateMachine(ref contextStateMachine);
            stack.Push(coreToken);
        }
    }
}
