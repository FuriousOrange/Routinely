using Routinely.Interop;

namespace Routinely;

/// <summary>
/// Represents a cooperatively scheduled unit of asynchronous work.
/// </summary>
/// <remarks>Coroutines created and
/// managed through this API can be resumed, awaited, or composed using methods such as WhenAll and WhenAny. The Coroutine struct
/// is designed to integrate with game loops, schedulers, or other systems that require cooperative control over
/// asynchronous execution.
/// </remarks>
public partial struct Coroutine
{
    internal class CoroutineVoid { }

    public delegate void ExceptionHandler(Exception ex);

    /// <summary>
    /// Gets the count of active coroutines from the currently active <see cref="CoroutineContext"/>.
    /// </summary>
    public static int Count => StackDispatcher.StackCount;

    static Coroutine()
    {
        ThreadInit();
    }

    /// <summary>
    /// Use to handle exceptions thrown by coroutines during resumption without needing to wrap
    /// <see cref="ResumeAll"/> in a try catch. Useful for logging or swallowing exceptions without crashing the application.
    /// </summary>
    /// <remarks>
    /// Use logging for exceptions.
    /// 
    /// <code>
    /// Coroutine.OnUnhandledException = ex => Logger.LogError(ex, "Unhandled exception in coroutine, see ex for details.");
    /// 
    /// async Coroutine Boom() => throw new Exception("Boom!");
    /// 
    /// Boom.Forget();
    /// 
    /// while(/* loop condition */)
    /// {
    ///     Coroutine.ResumeAll(); // Faults here!
    /// }
    /// </code>
    /// </remarks>
    public static ExceptionHandler? OnUnhandledException { get; set; }

    public static void ThreadInit() => StackDispatcher.ThreadInit();

    /// <summary>
    /// Resumes all coroutines until their next yield point.
    /// </summary>
    /// <returns>
    /// True if there are still pending coroutines; false if all coroutines have completed.
    /// </returns>
    /// <remarks>
    /// Use this method to drive the execution of coroutines from a central game loop or scheduler.
    /// Any execptions thrown by coroutines during resumuption are aggregated and thrown as an
    /// <see cref="AggregateException"/> at the end of the internal dipatch loop. 
    /// If an <see cref="OnUnhandledException"/> handler is set it will be used instead.
    /// <code>
    /// while (/* loop condition */)
    /// {
    ///     // Logic...
    ///     
    ///     Coroutine.ResumeAll();
    /// }
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ResumeAll() => StackDispatcher.MoveAllNext();

    /// <summary>
    /// Forces a coroutine to yield execution and resume on the next call to <see cref="ResumeAll"/>.
    /// </summary>
    /// <remarks>
    /// Use this method within a coroutine to yield execution and allow other coroutines to run.
    /// <code>
    /// async Coroutine Work()
    /// {
    ///     // Logic...
    ///     
    ///     await Coroutine.Yield();
    ///     
    ///     // Logic that runs after yielding...
    /// }
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YieldAwaiter Yield() => new();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine FromCustomStateMachine<TCustomStateMachine>(ref TCustomStateMachine customStateMachine)
        where TCustomStateMachine : struct, ICustomStateMachine
        => FromCustomStateMachine<TCustomStateMachine, CoroutineVoid>(ref customStateMachine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<TResult> FromCustomStateMachine<TCustomStateMachine, TResult>(ref TCustomStateMachine customStateMachine)
        where TCustomStateMachine : struct, ICustomStateMachine
    {
        var coreToken = StackDispatcher.GetCoroutineCoreToken();
        customStateMachine.ConfigureCoreToken(coreToken);

        coreToken.Item.StateMachine = CoroutineStateMachine<TCustomStateMachine>
            .CaptureStateMachine(ref customStateMachine);

        var stack = StackDispatcher.GetStack();
        stack.Push(coreToken);

        return new Coroutine<TResult>(coreToken, stack);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BridgeMoveNext<TMoveNextStateMachine, TCoroutine>(ref TMoveNextStateMachine moveNext, ref TCoroutine coroutine)
        where TMoveNextStateMachine : struct, IMoveNextStateMachine
        where TCoroutine : struct, ICoroutine
    {
        var coreToken = coroutine.CoreToken;

        IMoveNextBridgeBuilder bridge = MoveNextBridgeBuilder<TMoveNextStateMachine>.Start(ref moveNext, coreToken);

        coreToken.Item.StateMachine = coreToken.Item.StateMachine.Bridge(ref bridge);
    }

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is non allocating.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine WhenAll(params Coroutine[] coroutines) 
        => WhenAll<Coroutine, CoroutineVoid>(coroutines, null);

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is allocating. It creates a defensive array copy of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine WhenAll(IEnumerable<Coroutine> coroutines) 
        => WhenAll<Coroutine, CoroutineVoid>([.. coroutines], null);

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is allocating. It creates an array to hold the results of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T?[]> WhenAll<T>(params Coroutine<T>[] coroutines) 
        => WhenAll(coroutines, new T[coroutines.Length]);

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <param name="results">An array to hold the results of the provided coroutines. The length of this array must match the number of provided coroutines.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is non allocating.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T?[]> WhenAll<T>(Coroutine<T>[] coroutines, T[] results)
    => WhenAll<Coroutine<T>, T>(coroutines, results);

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is allocating. It creates a defensive array copy of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T?[]> WhenAll<T>(IEnumerable<Coroutine<T>> coroutines)
    {
        var coArray = coroutines.ToArray();
        return WhenAll(coArray, new T[coArray.Length]);
    }

    /// <summary>
    /// Creates a coroutine that completes when all of the provided coroutines have completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <param name="results">An array to hold the results of the provided coroutines. The length of this array must match the number of provided coroutines.</param>
    /// <returns>A coroutine representing the completion of all provided coroutines.</returns>
    /// <remarks>
    /// This method is allocating. It creates a defensive array copy of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T?[]> WhenAll<T>(IEnumerable<Coroutine<T>> coroutines, T[] results)
    {
        var coArray = coroutines.ToArray();
        return WhenAll(coArray, results);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult?[]> WhenAll<TCoroutine, TResult>(TCoroutine[] coroutines, TResult[]? results = null)
        where TCoroutine : struct, ICoroutine<TResult>
    {
        ArgumentNullException.ThrowIfNull(coroutines, nameof(coroutines));

        if (coroutines.Length == 0)
            throw new ArgumentException("At least one coroutine must be provided.", nameof(coroutines));

        if (results != null && coroutines.Length != results.Length)
            throw new ArgumentException("The length of the results array must match the number of coroutines.", nameof(results));

        ExchangeToken<WhenAllContext<TCoroutine, TResult>>? contextToken = null;

        var completedCount = 0;

        for (var i = 0; i < coroutines.Length; i++)
        {
            ref var coroutine = ref coroutines[i];

            coroutine.ThrowIfNoContext();
            //coroutine.ThrowIfNoDispatcherAffinity();

            if (coroutine.IsCompleted)
            {
                if (results != null)
                {
                    results[i] = coroutine.Result!;
                }

                completedCount++;
                continue;
            }

            ref var core = ref coroutine.CoreToken.Item;

            if (core.HasFlag(CoroutineCore.Awaited))
            {
                contextToken?.Return();
                AwaitedCoroutineException.ThrowMultipleAwait(coroutine);
            }

            core.SetFlag(CoroutineCore.Awaited);

            if (contextToken == null)
            {
                var context = new WhenAllContext<TCoroutine, TResult>(coroutines, results);
                contextToken = Exchange<WhenAllContext<TCoroutine, TResult>>.Put(context);
            }

            var moveNext = new WhenAllMoveNext<TCoroutine, TResult>(i, contextToken);
            BridgeMoveNext(ref moveNext, ref coroutine);
        }

        if (contextToken != null)
        {
            contextToken.Item.Count = coroutines.Length - completedCount;
        }

        if (completedCount == coroutines.Length)
        {
            contextToken?.Return();
            return FromResult(results!)!;
        }

        var stateMachine = new WhenAllStateMachine<TCoroutine, TResult>(contextToken!);

        return FromCustomStateMachine<WhenAllStateMachine<TCoroutine, TResult>, TResult?[]>(ref stateMachine);
    }

    /// <summary>
    /// Creates a coroutine that completes when one of the provided coroutines has completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine whose result is the coroutine that completed first.</returns>
    /// <remarks>
    /// This method is non allocating.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<Coroutine> WhenAny(params Coroutine[] coroutines) 
        => WhenAny<Coroutine, CoroutineVoid>(coroutines);

    /// <summary>
    /// Creates a coroutine that completes when one of the provided coroutines has completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine whose result is the coroutine that completed first.</returns>
    /// <remarks>
    /// This method is allocating. It creates a defensive array copy of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<Coroutine> WhenAny(IEnumerable<Coroutine> coroutines) 
        => WhenAny<Coroutine, CoroutineVoid>([.. coroutines]);

    /// <summary>
    /// Creates a coroutine that completes when one of the provided coroutines has completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine whose result is the coroutine that completed first.</returns>
    /// <remarks>
    /// This method is non allocating.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<Coroutine<TResult>> WhenAny<TResult>(params Coroutine<TResult>[] coroutines) 
        => WhenAny<Coroutine<TResult>, TResult>(coroutines);

    /// <summary>
    /// Creates a coroutine that completes when one of the provided coroutines has completed.
    /// </summary>
    /// <param name="coroutines">The coroutines to wait for.</param>
    /// <returns>A coroutine whose result is the coroutine that completed first.</returns>
    /// <remarks>
    /// This method is allocating. It creates a defensive array copy of the provided coroutines.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<Coroutine<TResult>> WhenAny<TResult>(IEnumerable<Coroutine<TResult>> coroutines) 
        => WhenAny<Coroutine<TResult>, TResult>([.. coroutines]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TCoroutine> WhenAny<TCoroutine, TResult>(TCoroutine[] coroutines)
        where TCoroutine : struct, ICoroutine<TResult>
    {
        ArgumentNullException.ThrowIfNull(coroutines, nameof(coroutines));

        if (coroutines.Length == 0)
        {
            throw new ArgumentException("At least one coroutine must be provided.", nameof(coroutines));
        }

        ExchangeToken<WhenAnyContext<TCoroutine, TResult>>? contextToken = null;

        for (var i = 0; i < coroutines.Length; i++)
        {
            var coroutine = coroutines[i];

            coroutine.ThrowIfNoContext();

            if (coroutine.IsCompleted)
            {
                contextToken?.Return();
                return FromResult(coroutine);
            }

            ref var core = ref coroutine.CoreToken.Item;

            if (core.HasFlag(CoroutineCore.Awaited))
            {
                contextToken?.Return();
                AwaitedCoroutineException.ThrowMultipleAwait(coroutine);
            }

            core.SetFlag(CoroutineCore.Awaited);

            if (contextToken == null)
            {
                var context = new WhenAnyContext<TCoroutine, TResult>(coroutines);
                contextToken = Exchange<WhenAnyContext<TCoroutine, TResult>>.Put(context);
            }

            var moveNext = new WhenAnyMoveNext<TCoroutine, TResult>(contextToken, ref coroutine);
            BridgeMoveNext(ref moveNext, ref coroutine);
        }

        var stateMachine = new WhenAnyStateMachine<TCoroutine, TResult>(contextToken!);

        return FromCustomStateMachine<WhenAnyStateMachine<TCoroutine, TResult>, TCoroutine>(ref stateMachine);
    }

    /// <summary>
    /// Gets a coroutine that has already completed.
    /// </summary>
    public static Coroutine CompletedCoroutine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new();
    }

    /// <summary>
    /// Gets a coroutine that has already completed with the provided result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The result to return.</param>
    /// <returns>A coroutine that has already completed with the provided result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<TResult> FromResult<TResult>(TResult result)
    {
        return new Coroutine<TResult>(result);
    }

    /// <summary>
    /// Yields execution of the current coroutine. On the next <see cref="ResumeAll"/> call the coroutine switches to the coroutine generated by the next delegate.
    /// </summary>
    /// <param name="next">A delegate representing the coroutine to switch to.</param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method to switch from one coroutine to another without returning to the caller, useful for 
    /// state machines.
    /// The entire call chain of the switching coroutine is terminated, so this is a one way trip.
    /// This method will allocate based on how the next delegate is captured. If the next delegate captures any variables, including the 'this' reference, it will allocate. If the next delegate is static it will not allocate.
    /// See <see cref="SwitchTo{TContext}(TContext, Func{TContext, Coroutine})"/> for passing context without allocating.
    /// <code>
    /// static async Coroutine Producer()
    /// {
    ///     // Logic...
    ///     
    ///     await Coroutine.SwitchTo(Consumer);
    /// }
    /// 
    /// static async Coroutine Consumer()
    /// {
    ///     // Logic...
    ///     
    ///     await Coroutine.SwitchTo(Producer);
    /// }
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SwitchToAwaiter<TCoroutine> SwitchTo<TCoroutine>(Func<TCoroutine> next)
        where TCoroutine: struct, ICoroutine
        => new(next);

    /// <summary>
    /// Yields execution of the current coroutine. On the next <see cref="ResumeAll"/> call the coroutine switches to the coroutine generated by the next delegate.
    /// </summary>
    /// <param name="context">The context to pass to the next coroutine.</param>
    /// <param name="next">A delegate representing the coroutine to switch to.</param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method to switch from one coroutine to another without returning to the caller, useful for 
    /// state machines.
    /// The entire call chain of the switching coroutine is terminated, so this is a one way trip.
    /// This method is intended to provide a way to pass context to the next coroutine without allocating.
    /// <code>
    /// async Coroutine Producer()
    /// {
    ///     // Logic...
    ///     
    ///     await Coroutine.SwitchTo(this, @this => @this.Consumer());
    /// }
    /// 
    /// async Coroutine Consumer()
    /// {
    ///     // Logic...
    ///     
    ///     await Coroutine.SwitchTo(this, @this => @this.Producer());
    /// }
    /// </code>
    /// </remarks>
    public static SwitchToAwaiter<TContext, TCoroutine> SwitchTo<TContext, TCoroutine>(TContext context, Func<TContext, TCoroutine> next)
        where TCoroutine: struct, ICoroutine
        => new(context, next);

    /// <summary>
    /// Wraps a <see cref="Task"/> in a coroutine. The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="task">The task to wrap in a coroutine.</param>
    /// <returns>A coroutine representing the task.</returns>
    public static Coroutine FromTask(Task task)
    {
        static async Coroutine TaskWrapper(Task task)
        {
            while (!task.IsCompleted)
            {
                await Yield();
            }

            task.GetAwaiter().GetResult();
        }

        return TaskWrapper(task);
    }

    /// <summary>
    /// Wraps a <see cref="Task"/> started from the <see cref="taskFactory"/> parameter in a coroutine. 
    /// The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="taskFactory">A delegate that starts a task with the provided <see cref="CancellationToken"/>.</param>
    /// <returns>A coroutine representing the task.</returns>
    /// <remarks>
    /// This method creates an internal cancellation source that is canceled when the coroutine is canceled. 
    /// The cancellation token from this source is passed to the provided task factory allowing the task to be canceled cooperatively when the coroutine is canceled.
    /// <code>
    /// async Task TaskWork(CancellationToken ct)
    /// {
    ///     while(!ct.IsCancellationRequested)
    ///     {
    ///         // Logic...
    ///     }
    /// }
    /// 
    /// async Coroutine TaskConsumer()
    /// {
    ///     await Coroutine.FromTask(ct => TaskWork(ct));
    /// }
    /// 
    /// var taskConsumerCo = TaskConsumer();
    /// 
    /// taskConsumerCo.Cancel(); // Cancel the coroutine and the task it depends on.
    /// </code>
    /// </remarks>
    public static Coroutine FromTask(Func<CancellationToken, Task> taskFactory)
    {
        async Coroutine TaskCancellationWrapper()
        {
            using var cancellationSource = new CancellationTokenSource();
            using var ctc = await CancellationContract.Enter();

            var task = taskFactory(cancellationSource.Token);

            try
            {
                await FromTask(task).Enforce(ctc);
            }
            finally
            {
                cancellationSource.Cancel();
            }
        }

        return TaskCancellationWrapper();
    }


    /// <summary>
    /// Wraps a <see cref="Task{TResult}"/> in a coroutine. The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="task">The task to wrap in a coroutine.</param>
    /// <returns>A coroutine representing the task.</returns>
    public static Coroutine<TResult> FromTask<TResult>(Task<TResult> task)
    {
        static async Coroutine<TResult> TaskWrapper(Task<TResult> task)
        {
            while (!task.IsCompleted)
            {
                await Yield();
            }

            return task.GetAwaiter().GetResult();
        }

        return TaskWrapper(task);
    }

    /// <summary>
    /// Wraps a <see cref="Task{TResult}"/> started from the <see cref="taskFactory"/> parameter in a coroutine. 
    /// The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="taskFactory">A delegate that starts a task with the provided <see cref="CancellationToken"/>.</param>
    /// <returns>A coroutine representing the task.</returns>
    /// <remarks>
    /// This method creates an internal cancellation source that is canceled when the coroutine is canceled. 
    /// The cancellation token from this source is passed to the provided task factory allowing the task to be canceled cooperatively when the coroutine is canceled.
    /// </remarks>
    public static Coroutine<TResult> FromTask<TResult>(Func<CancellationToken, Task<TResult>> taskFactory)
    {
        async Coroutine<TResult> TaskCancellationWrapper()
        {
            using var cancellationSource = new CancellationTokenSource();
            using var ctc = await CancellationContract.Enter();

            var task = taskFactory(cancellationSource.Token);

            try
            {
                return await FromTask(task).Enforce(ctc);
            }
            finally
            {
                cancellationSource.Cancel();
            }
        }

        return TaskCancellationWrapper();
    }

    /// <summary>
    /// Wraps a <see cref="ValueTask"/> in a coroutine. The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="valueTask">The task to wrap in a coroutine.</param>
    /// <returns>A coroutine representing the task.</returns>
    public static Coroutine FromValueTask(ValueTask valueTask)
    {
        if(valueTask.IsCompleted)
        {
            valueTask.GetAwaiter().GetResult();
            return CompletedCoroutine;
        }
        else
        {
            return FromTask(valueTask.AsTask());
        }
    }


    /// <summary>
    /// Wraps a <see cref="ValueTask"/> started from the <see cref="taskFactory"/> parameter in a coroutine. 
    /// The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="taskFactory">A delegate that starts a task with the provided <see cref="CancellationToken"/>.</param>
    /// <returns>A coroutine representing the task.</returns>
    /// <remarks>
    /// This method creates an internal cancellation source that is canceled when the coroutine is canceled. 
    /// The cancellation token from this source is passed to the provided task factory allowing the task to be canceled cooperatively when the coroutine is canceled.
    /// <code>
    /// async ValueTask ValueTaskWork(CancellationToken ct)
    /// {
    ///     while(!ct.IsCancellationRequested)
    ///     {
    ///         // Logic...
    ///     }
    /// }
    /// 
    /// async Coroutine ValueTaskConsumer()
    /// {
    ///     await Coroutine.FromValueTask(ct => ValueTaskWork(ct));
    /// }
    /// 
    /// var valueTaskConsumerCo = ValueTaskConsumer();
    /// 
    /// valueTaskConsumerCo.Cancel(); // Cancel the coroutine and the task it depends on.
    /// </code>
    /// </remarks>
    public static Coroutine FromValueTask(Func<CancellationToken, ValueTask> valueTaskFactory)
    {
        Task taskFactory(CancellationToken ct) => valueTaskFactory(ct).AsTask();
        return FromTask(taskFactory);
    }

    /// <summary>
    /// Wraps a <see cref="ValueTask{TResult}"/> in a coroutine. The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="valueTask">The task to wrap in a coroutine.</param>
    /// <returns>A coroutine representing the task.</returns>
    public static Coroutine<TResult> FromValueTask<TResult>(ValueTask<TResult> valueTask)
    {
        if (valueTask.IsCompleted)
        {
            return FromResult(valueTask.GetAwaiter().GetResult());
        }
        else
        {
            return FromTask(valueTask.AsTask());
        }
    }

    /// <summary>
    /// Wraps a <see cref="ValueTask{TResult}"/> started from the <see cref="taskFactory"/> parameter in a coroutine. 
    /// The returned coroutine will complete when the task completes. 
    /// If the task faults the coroutine will fault with the same exception. 
    /// If the task is canceled the coroutine will be canceled.
    /// </summary>
    /// <param name="taskFactory">A delegate that starts a task with the provided <see cref="CancellationToken"/>.</param>
    /// <returns>A coroutine representing the task.</returns>
    /// <remarks>
    /// This method creates an internal cancellation source that is canceled when the coroutine is canceled. 
    /// The cancellation token from this source is passed to the provided task factory allowing the task to be canceled cooperatively when the coroutine is canceled.
    /// </remarks>
    public static Coroutine<TResult> FromValueTask<TResult>(Func<CancellationToken, ValueTask<TResult>> valueTaskFactory)
    {
        Task<TResult> taskFactory(CancellationToken ct) => valueTaskFactory(ct).AsTask();
        return FromTask(taskFactory);
    }

    /// <summary>
    /// Creates a new coroutine context. A coroutine context allows for the logical grouping of coroutines.
    /// Use with <see cref="Context"/> inside a coroutine to set it's execution context.
    /// <remarks>
    /// Use a <see cref="CoroutineContext"/> to group coroutines that have different execution lifecyles.
    /// <code>
    /// var highPriority = Coroutine.CreateContext();
    /// var lowPriority = Coroutine.CreateContext();
    /// 
    /// async Coroutine HighPriorityWork()
    /// {
    ///     // Force work into high priority context
    ///     await Coroutine.Context(highPriority);
    ///     
    ///     // Logic...
    /// }
    /// 
    /// async Coroutine LowPriorityWork()
    /// {
    ///     // Force work into low priority context
    ///     await Coroutine.Context(lowPriority);
    ///     
    ///     // Logic...
    /// }
    /// 
    /// // Schedule some work in both contexts
    /// while(/* loop condition */)
    /// {
    ///     // Schedule high priority work
    ///     Coroutine.SetContext(highPriority);
    ///     Coroutine.ResumeAll();
    ///     
    ///     // Schedule low priority work
    ///     Coroutine.SetContext(lowPriority);
    ///     Coroutine.ResumeAll();
    /// }
    /// </code>
    /// </remarks>
    /// </summary>
    /// <returns></returns>
    public static CoroutineContext CreateContext() => StackDispatcher.CreateContext();

    /// <summary>
    /// Set the current coroutine context from a previously created <see cref="CreateContext"/>.
    /// </summary>
    /// <param name="context">The context to set.</param>
    public static void SetContext(CoroutineContext context) => StackDispatcher.SetContext(context);

    /// <summary>
    /// Disposes all coroutine contexts .
    /// This will restore the default context and drop all other contexts. 
    /// Use this method to clean up any contexts created with <see cref="CreateContext"/> when they are no longer needed.
    /// </summary>
    public static void DisposeContexts()
    {
        if(StackDispatcher.Contexts.Count == 0)
        {
            return;
        }

        // Restore default context
        SetContext(StackDispatcher.Contexts[0]);

        // Drop the rest
        StackDispatcher.Contexts.Clear();
    }

    /// <summary>
    /// Allows for the execution and migration of coroutines between contexts created by <see cref="CreateContext"/>.
    /// </summary>
    /// <param name="context">The context that the coroutine should execute or migrate to.</param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method to force a coroutine to execute within a specific context. If the context is the same as the current context, execution continues as normal. 
    /// If the context is different from the current context, the coroutine will yield and will resume execution on the next <see cref="ResumeAll"/> call that has that context set.
    /// Internally this will move the entire coroutine execution stack to the provided context. This method can be used to migrate a coroutine
    /// between different contexts over it's lifecyle.
    /// <code>
    /// var contextA = Coroutine.CreateContext();
    /// var contextB = Coroutine.CreateContext();
    /// var contextC = Coroutine.CreateContext();
    /// 
    /// async Coroutine SpreadWork()
    /// {
    ///     await Coroutine.Context(contextA);
    ///     
    ///     // Context A logic...
    /// 
    ///     await Coroutine.Context(contextB);
    ///     
    ///     // Context B logic...
    ///     
    ///     await Coroutine.Context(contextC);
    ///     
    ///     // Context C logic...
    /// }
    /// </code>
    /// </remarks>
    public static ContextAwaiter Context(CoroutineContext context) => new(context);
}