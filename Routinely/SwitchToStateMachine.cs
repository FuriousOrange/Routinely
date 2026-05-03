namespace Routinely;

internal interface ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine

{
    bool HasYielded { get; set; }

    ExchangeToken<CoroutineCore> CoreToken { get; set; }

    CoroutineStack Stack { get; }

    TCoroutine Next { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CollapseStack<TSwitchTo>(ref TSwitchTo switchTo)
        where TSwitchTo : ISwitchTo<TCoroutine>
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
    internal static void MoveNext<TSwitchTo>(ref TSwitchTo switchTo)
        where TSwitchTo : ISwitchTo<TCoroutine>
    {
        if (!switchTo.HasYielded)
        {
            try
            {
                switchTo.HasYielded = true;
                var nextCo = switchTo.Next;

                HandleSwitch(ref switchTo, ref nextCo);
            }
            catch (Exception ex)
            {
                switchTo.CoreToken.Item.Fault(ex);
                throw;
            }
        }
        else
        {
            switchTo.CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void MoveNextWithContext<TSwitchTo, TContext>(ref TSwitchTo switchTo)
        where TSwitchTo : ISwitchTo<TCoroutine>
    {
        if (!switchTo.HasYielded)
        {
            try
            {
                switchTo.HasYielded = true;
                var nextCo = switchTo.Next;

                HandleSwitch(ref switchTo, ref nextCo);
            }
            catch (Exception ex)
            {
                switchTo.CoreToken.Item.Fault(ex);
                throw;
            }
        }
        else
        {
            switchTo.CoreToken.Item.SetFlag(CoroutineCore.Completed);
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
        where TSwitchTo : ISwitchTo<TCoroutine>
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


//internal interface ISwitchTo<TCoroutine> : ISwitchTo
//    where TCoroutine : struct, ICoroutine
//{

//}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct SwitchToStateMachine<TCoroutine>(Func<TCoroutine> next) : IAsyncStateMachine, ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine
{
    private readonly Func<TCoroutine> next = next;

    public bool HasYielded
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

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

    public readonly TCoroutine Next
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => next();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext() => ISwitchTo<TCoroutine>.MoveNext(ref this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct SwitchToStateMachine<TContext, TCoroutine>(TContext context, Func<TContext, TCoroutine> next) : IAsyncStateMachine, ISwitchTo<TCoroutine>
    where TCoroutine : struct, ICoroutine
{
    private readonly Func<TContext, TCoroutine> next = next;

    public bool HasYielded
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    public TContext Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = context;


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

    public readonly TCoroutine Next
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => next(Context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext() => ISwitchTo<TCoroutine>.MoveNext(ref this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}
