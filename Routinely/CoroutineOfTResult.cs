using static Routinely.Coroutine;

namespace Routinely;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
public partial struct Coroutine<TResult> : ICoroutine<TResult>, ICoroutineNotifyAwaited<TResult>
{
    internal bool isCompleted = true;
    internal TResult? result;

    /// <summary>
    /// Returns true if the coroutine is active; otherwise returns false.
    /// </summary>
    /// <remarks>
    /// A coroutine is considered to have context if it is currently being executed or is awaiting.
    /// Once a coroutine is completed it will have context for one further tick to allow user code to observe it's state.
    /// If a coroutine has been forgotten using <see cref="CoroutineExtensions.Forget{TCoroutine}"/> then it will be 
    /// immediately recycled upon completion and will no longer have context. Use this property to ensure coroutine handles
    /// are valid before checking other properties if the handle has been held for a long time.
    /// <code>
    /// var coroutine = SomeCoroutine();
    /// 
    /// // Time passes...
    /// 
    /// if (coroutine.HasContext)
    /// {
    ///     // Safe to check other properties
    /// }
    /// </code>
    /// </remarks>
    public readonly bool HasContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => isCompleted || ICoroutine.Contextful(CoreToken, Id);
    }

    /// <summary>
    /// Returns true if a coroutine has completed; otherwise false.
    /// </summary>
    /// <remarks>
    /// A coroutine is considered to be completed if it finishes execution, is canceled, or has faulted. 
    /// Once a coroutine is completed it will have context for one further tick to allow user code to observe it's state.
    /// </remarks>
    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => isCompleted || ICoroutine.HasFlag(CoreToken, CoroutineCore.Completed, Id);

    }

    /// <summary>
    /// Returns true if a coroutine has faulted due to an exception; otherwise false.
    /// </summary>
    /// <remarks>
    /// A faulted coroutine will also have it's <see cref="IsCompleted"/> property return true. 
    /// Once a coroutine is completed it will have context for one further tick to allow user code to observe it's state.
    /// </remarks>
    public readonly bool IsFaulted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !isCompleted && ICoroutine.HasFlag(CoreToken, CoroutineCore.Faulted, Id);
    }

    /// <summary>
    /// Returns true if a coroutine has been canceled; otherwise false.
    /// </summary>
    /// <remarks>
    /// A canceled coroutine will also have it's <see cref="IsCompleted"/> property return true.
    /// Once a coroutine is completed it will have context for one further tick to allow user code to observe it's state.
    /// </remarks>
    public readonly bool IsCanceled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !isCompleted && ICoroutine.HasFlag(CoreToken, CoroutineCore.Canceled, Id);
    }

    /// <summary>
    /// Returns true if a coroutine has a cancellation contract; otherwise false.
    /// </summary>
    /// <remarks>
    /// This property is useful to understand whether a coroutine has expected resource cleanup on cancellation.
    /// </remarks>
    public readonly bool CancelationContract
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !isCompleted && ICoroutine.HasFlag(CoreToken, CoroutineCore.CancellationContract, Id);
    }

    /// <summary>
    /// Returns true if a coroutine has been awaited by another coroutine; otherwise false.
    /// </summary>
    /// <remarks>
    /// This property is useful to understand whether a coroutine's stack has been merged into another coroutine. 
    /// Awaited coroutines are merged into the callstack of their awaiter and will not be scheduled independently.
    /// </remarks>
    public readonly bool IsAwaited
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !isCompleted && ICoroutine.HasFlag(CoreToken, CoroutineCore.Awaited, Id);
    }

    /// <summary>
    /// Returns the exception that caused the coroutine to fault, or null if the coroutine has not faulted.
    /// </summary>
    /// <remarks>
    /// Use in conjunction with the <see cref="IsFaulted"/> property to determine if a coroutine has faulted and to observe the exception that caused the fault.
    /// <code>
    /// var coroutine = SomeCoroutine();
    /// 
    /// if(coroutine.IsFaulted)
    /// {
    ///     // Exception will return a result.
    ///     var ex = coroutine.Exception;
    /// }
    /// </code>
    /// </remarks>
    public readonly Exception? Exception => GetException();

    /// <summary>
    /// The unique identifer for this coroutine instance.
    /// </summary>
    /// <remarks>
    /// Used to track whether a coroutine has context.
    /// </remarks>
    public uint Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    /// <summary>
    /// Returns the result of a completed coroutine; otherwise returns default.
    /// </summary>
    /// <remarks>
    /// To ensure the result is correct the <see cref="IsCompleted"/> property should be checked before accessing the result. 
    /// If a coroutine has not completed, this property will return the default value for <see cref="{TResult}"/>.
    /// </remarks>
    public readonly TResult? Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetResult();
    }

    /// <summary>
    /// Gets the internal pooled token used to manage a coroutine's state.
    /// </summary>
    /// <remarks>
    /// The core token is used internally to track it's internal <see cref="CoroutineCore"/> state.
    /// </remarks>
    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    } = null!;

    /// <summary>
    /// Gets the coroutine stack used to manage the execution state of this coroutine.
    /// </summary>
    /// <remarks>The stack maintains the state required for coroutine execution and is intended for internal
    /// use; external code should not attempt to modify the stack
    /// directly.</remarks>
    public CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coroutine()
    {
        Id = CoroutineId.GetNextId();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coroutine(TResult result) : this()
    {
        this.result = result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Coroutine(ExchangeToken<CoroutineCore> coreToken, CoroutineStack stack) : this()
    {
        isCompleted = false;
        CoreToken = coreToken;
        coreToken.Item.Id = Id;
        Stack = stack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Coroutine<TResult> GetAwaiter()
    {
        if (isCompleted)
            return this;

        if (!HasContext)
        {
            throw new NoContextException();
        }

        ref var core = ref CoreToken.Item;

        if (core.HasFlag(CoroutineCore.Awaited))
        {
            AwaitedCoroutineException.ThrowMultipleAwait(this);
        }

        core.SetFlag(CoroutineCore.Awaited);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TResult GetResult()
    {
        if (isCompleted)
        {
            return result!;
        }

        ref var core = ref CoreToken.Item;

        if (core.HasFlag(CoroutineCore.Faulted))
        {
            core.Throw();
        }

        if (core.Result == null)
            return default!;

        return core.Result.UnsafeGet<TResult>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly void ICoroutineNotifyAwaiting.GetResult() { }

    readonly internal Exception? GetException()
    {
        ref var core = ref CoreToken.Item;

        ExceptionBox exceptionBox = default;

        if (core.Result == null)
            return default;

        core.Result.CheckedGet(ref exceptionBox);

        if (exceptionBox.DispatchInfo == null)
            return default;

        return exceptionBox.DispatchInfo.SourceException;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly void ICoroutineNotifyAwaiting.OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
    {
        if (coroutine.IsCompleted)
        {
            coroutine.Configure(ref stateMachine, awaiter.Stack);
        }
        else
        {
            StackDispatcher.MergeActive(awaiter.Stack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Configure<TStateMachine>(ref TStateMachine stateMachine, CoroutineStack stack)
        where TStateMachine : IAsyncStateMachine
    {
        var token = StackDispatcher.GetCoroutineCoreToken();
        ref var core = ref token.Item;
        core.Id = Id;
        CoreToken = token;
        Stack = stack;

        stack.Bump(CoreToken);

        isCompleted = false;

        core.StateMachine = CoroutineStateMachine<TStateMachine>.CaptureStateMachine(ref stateMachine);
    }
}