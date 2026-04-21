namespace Routinely;

internal interface ISwitchTo
{
    ExchangeToken<CoroutineCore> CoreToken { get; set; }

    CoroutineStack Stack { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CollapseStack<TSwitchTo>(ref TSwitchTo switchTo)
        where TSwitchTo : ISwitchTo
    {
        var currentStack = StackDispatcher.CurrentStack;

        if (currentStack == null)
        {
            return;
        }

        ExchangeToken<CoroutineCore> coreToken = switchTo.CoreToken;

        if (coreToken != currentStack.Tokens[0])
        {
            ref var rootCore = ref currentStack.Tokens[0].Item;
            ref var core = ref coreToken.Item;
            var stateMachine = core.StateMachine;

            core.Flags = rootCore.Flags;
            core.StateMachine = rootCore.StateMachine;

            rootCore.ClearFlag(CoroutineCore.Awaited);
            rootCore.StateMachine = stateMachine;
            switchTo.CoreToken = currentStack.Tokens[0];
        }

        while (currentStack.HeadIndex - 1 != 0)
        {
            var disposingCoreToken = currentStack.Peek();
            ref var disposingCore = ref disposingCoreToken.Item;

            disposingCore.Cancel();
            disposingCore.Free();
            disposingCoreToken.Return();
            currentStack.Pop();
        }
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct SwitchToStateMachine(Func<Coroutine> next) : IAsyncStateMachine, ISwitchTo
{
    private bool hasYielded = false;
    private readonly Func<Coroutine> next = next;

    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    public CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext()
    {
        if (!hasYielded)
        {
            try
            {
                hasYielded = true;

                var next = this.next();

                if (next.IsCompleted)
                {
                    ISwitchTo.CollapseStack(ref this);

                    CoreToken.Item.SetFlag(CoroutineCore.Completed);
                }
                else
                {
                    ISwitchTo.CollapseStack(ref this);

                    if (StackDispatcher.CurrentContext == next.Stack.CoroutineContext)
                    {
                        StackDispatcher.MergeActive(next.Stack);
                    }
                    else
                    {
                        StackDispatcher.CurrentStack.MigrateStack(next.Stack);
                    }

                    next.CoreToken.Item.SetFlag(CoroutineCore.Awaited | CoroutineCore.TailCall);
                }
            }
            catch (Exception ex)
            {
                CoreToken.Item.Fault(ex);
            }
        }
        else
        {
            CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct SwitchToStateMachine<TContext>(TContext context, Func<TContext, Coroutine> next) : IAsyncStateMachine, ISwitchTo
{
    private bool hasYielded = false;
    private readonly TContext context = context;
    private readonly Func<TContext, Coroutine> next = next;

    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    public CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext()
    {
        if (!hasYielded)
        {
            try
            {
                hasYielded = true;

                var next = this.next(context);

                if (next.IsCompleted)
                {
                    ISwitchTo.CollapseStack(ref this);

                    CoreToken.Item.SetFlag(CoroutineCore.Completed);
                }
                else
                {
                    ISwitchTo.CollapseStack(ref this);

                    if (StackDispatcher.CurrentContext == next.Stack.CoroutineContext)
                    {
                        StackDispatcher.MergeActive(next.Stack);
                    }
                    else
                    {
                        StackDispatcher.CurrentStack.MigrateStack(next.Stack);
                    }

                    next.CoreToken.Item.SetFlag(CoroutineCore.Awaited | CoroutineCore.TailCall);
                }
            }
            catch (Exception ex)
            {
                CoreToken.Item.Fault(ex);
            }
        }
        else
        {
            CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}
