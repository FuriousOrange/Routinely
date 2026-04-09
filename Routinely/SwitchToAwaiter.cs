namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct SwitchToAwaiter(Func<Coroutine> next) : ICoroutineNotifyAwaiting
{
    private readonly Func<Coroutine> next = next;

    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public readonly CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => null!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetResult()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly SwitchToAwaiter GetAwaiter() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TCoroutine : struct, ICoroutine
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
    {
        if (coroutine.IsCompleted)
        {
            // We can forgo the current statemachine here in favour of the root being a SwitchToStateMachine
            var switchTo = new SwitchToStateMachine(next);
            coroutine.Configure(ref switchTo, StackDispatcher.GetStack());
            switchTo.CoreToken = coroutine.CoreToken;
            switchTo.Stack = coroutine.Stack;
            CoroutineStateMachine<SwitchToStateMachine>.Update(coroutine.CoreToken.Item.StateMachine, ref switchTo);
        }
        else
        {
            // Otherwise just use the current stack
            var stack = StackDispatcher.CurrentStack;
            var coreToken = StackDispatcher.GetCoroutineCoreToken();
            var switchTo = new SwitchToStateMachine(next)
            {
                CoreToken = coreToken,
                Stack = stack
            };
            ref var core = ref coreToken.Item;
            core.SetFlag(CoroutineCore.Awaited);
            core.StateMachine = CoroutineStateMachine<SwitchToStateMachine>.CaptureStateMachine(ref switchTo);
            stack.Push(coreToken);
        }
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct SwitchToAwaiter<TContext>(TContext context, Func<TContext, Coroutine> next) : ICoroutineNotifyAwaiting
{
    private readonly Func<TContext, Coroutine> next = next;
    private readonly TContext context = context;

    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public readonly CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => null!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetResult()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly SwitchToAwaiter<TContext> GetAwaiter() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TCoroutine : struct, ICoroutine
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
    {
        if (coroutine.IsCompleted)
        {
            // We can forgo the current statemachine here in favour of the root being a SwitchToStateMachine
            var ym = new SwitchToStateMachine<TContext>(context, next);
            coroutine.Configure(ref ym, StackDispatcher.GetStack());
            ym.CoreToken = coroutine.CoreToken;
            ym.Stack = coroutine.Stack;
            CoroutineStateMachine<SwitchToStateMachine<TContext>>.Update(coroutine.CoreToken.Item.StateMachine, ref ym);
        }
        else
        {
            // Otherwise just use the current stack
            var stack = StackDispatcher.CurrentStack;
            var coreToken = StackDispatcher.GetCoroutineCoreToken();
            var ym = new SwitchToStateMachine<TContext>(context, next)
            {
                CoreToken = coreToken,
                Stack = stack
            };
            ref var core = ref coreToken.Item;
            core.SetFlag(CoroutineCore.Awaited);
            core.StateMachine = CoroutineStateMachine<SwitchToStateMachine<TContext>>.CaptureStateMachine(ref ym);
            stack.Push(coreToken);
        }
    }
}