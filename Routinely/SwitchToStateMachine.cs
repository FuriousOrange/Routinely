namespace Routinely;

internal interface ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine

{
    ExchangeToken<CoroutineCore> CoreToken { get; set; }

    CoroutineStack Stack { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CollapseStack<TSwitchTo>(ref TSwitchTo switchTo)
        where TSwitchTo : struct, ISwitchTo<TCoroutine>
    {
        var currentStack = StackDispatcher.CurrentStack;

        ExchangeToken<CoroutineCore> coreToken = switchTo.CoreToken;

        ref var rootCore = ref currentStack.Tokens[0].Item;
        ref var core = ref coreToken.Item;
        var stateMachine = core.StateMachine;

        core.Flags = rootCore.Flags;
        core.StateMachine = rootCore.StateMachine;

        rootCore.ClearFlag(CoroutineCore.Awaited);
        rootCore.StateMachine = stateMachine;
        switchTo.CoreToken = currentStack.Tokens[0];

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void PersistContext(ref TCoroutine next)
    {
        if (StackDispatcher.CurrentContext == next.Stack.CoroutineContext)
        {
            StackDispatcher.MergeActive(next.Stack);
        }
        else
        {
            var stack = StackDispatcher.DetachActive();
            var nextContext = next.Stack.CoroutineContext;

            // Move the active stack over to the new context and merge it with the next stack
            nextContext.MigrateStack(stack);
            stack.MergeStack(next.Stack);

            // Return the next stack to the pool and detach it from the next context
            nextContext.DetachStack(next.Stack);
            next.Stack.StackToken.Return(false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void HandleSwitch<TSwitchTo>(ref TSwitchTo switchTo, ref TCoroutine next)
        where TSwitchTo : struct, ISwitchTo<TCoroutine>
    {
        next.ThrowIfNoContext();

        if (next.IsCompleted)
        {
            CollapseStack(ref switchTo);

            switchTo.CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
        else
        {
            CollapseStack(ref switchTo);
            PersistContext(ref next);

            next.CoreToken.Item.SetFlag(CoroutineCore.Awaited | CoroutineCore.TailCall);
        }
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct SwitchToStateMachine<TCoroutine>(Func<TCoroutine> next) : IAsyncStateMachine, ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine
{
    private readonly Func<TCoroutine> next = next;
    private bool hasYielded = false;

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
                var nextCo = next();

                ISwitchTo<TCoroutine>.HandleSwitch(ref this, ref nextCo);
            }
            catch (Exception ex)
            {
                CoreToken.Item.Fault(ex);
                throw;
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
internal struct SwitchToStateMachine<TContext, TCoroutine>(TContext context, Func<TContext, TCoroutine> next) : IAsyncStateMachine, ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine
{
    private readonly TContext context = context;
    private readonly Func<TContext, TCoroutine> next = next;
    private bool hasYielded = false;

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
                var nextCo = next(context);

                ISwitchTo<TCoroutine>.HandleSwitch(ref this, ref nextCo);
            }
            catch (Exception ex)
            {
                CoreToken.Item.Fault(ex);
                throw;
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
