using System.Diagnostics.CodeAnalysis;

namespace Routinely;

internal static class StackDispatcher
{
    private static readonly object threadInitLock = new();

    private static byte currentId;

    [ThreadStatic]
    private static bool initialized;

    [ThreadStatic]
    internal static byte Id;

    [ThreadStatic]
    internal static int StackCount;

    [ThreadStatic]
    internal static PartitionArray<CoroutineStack> Stacks;

    [ThreadStatic]
    internal static List<CoroutineContext> Contexts;

    [ThreadStatic]
    internal static CoroutineContext MainContext;

    [ThreadStatic]
    internal static CoroutineContext CurrentContext;

    [ThreadStatic]
    internal static CoroutineStack? CurrentStack;

    [ThreadStatic]
    internal static int CurrentIndex;

    internal static double DeltaSeconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CurrentContext.DeltaSeconds;
    }

    internal static TimeSpan DeltaTimeSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CurrentContext.DeltaTimeSpan;
    }

    static StackDispatcher()
    {
        ThreadInit();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [MemberNotNull(nameof(Stacks), nameof(Contexts), nameof(MainContext), nameof(CurrentContext))]
    static internal void ThreadInit()
    {
        lock (threadInitLock)
        {
            if (initialized)
            {
                if (Stacks != null && Contexts != null && MainContext != null && CurrentContext != null)
                {
                    return;
                }
                else
                {
                    throw new InvalidOperationException("StackDispatcher thread initialization failed");
                }
            }

            Id = ++currentId;
            Stacks ??= new();
            CurrentStack = null!;
            MainContext ??= new(Stacks, StackCount, CurrentIndex, 0);
            CurrentContext ??= MainContext;
            Contexts ??= [MainContext];
            initialized = true;
        }
    }

    static internal void ThreadCleanup()
    {
        lock (threadInitLock)
        {
            if (!initialized)
            {
                return;
            }

            StackCount = 0;
            CurrentStack = null!;
            CurrentContext = null!;
            MainContext = null!;
            Stacks = null!;
            Contexts = null!;
            StackCount = 0;
            CurrentIndex = 0;
            initialized = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineStack GetStack()
    {
        var stack = CoroutineStack.Get();

        if (StackCount >= Stacks.Length)
        {
            Stacks.Expand();
        }

        stack.DispatcherIndex = StackCount;
        stack.CoroutineContext = CurrentContext!;
        Stacks[StackCount++] = stack;

        CurrentContext.StackCount = StackCount;

        return stack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExchangeToken<CoroutineCore> GetCoroutineCoreToken()
    {
        var token = Exchange<CoroutineCore>.Put(new CoroutineCore
        {
            DispatcherId = Id
        });
        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void MergeActive(CoroutineStack awaitStack)
    {
        CurrentStack?.MergeStack(awaitStack);
        ReturnStack(awaitStack);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StackMoveNext(CoroutineStack stack)
    {
        // Need to capture the current active stack here as the previous stack
        // to ensure that when we recursively call StackMoveNext
        // we don't lose the stack execution context of the current stack.
        var prevStack = CurrentStack;
        CurrentStack = stack;

        var coreToken = stack.Peek();
        ref var core = ref coreToken.Item;

        if (core.HasFlag(CoroutineCore.Completed) == false)
        {
            core.StateMachine.MoveNext();

            if (core.HasFlag(CoroutineCore.Completed | CoroutineCore.Awaited))
            {
                goto moveNext;
            }

            goto end;
        }

    moveNext:
        if (core.HasFlag(CoroutineCore.Faulted))
        {
            ExceptionBox exceptionBox = default;
            core.Result?.CheckedGet(ref exceptionBox);

            if (core.HasFlag(CoroutineCore.TailCall))
            {
                stack.Tokens[0].Item.Fault(exceptionBox);
                stack.Exception = exceptionBox.DispatchInfo?.SourceException;
            }
            else if (!core.HasFlag(CoroutineCore.Awaited))
            {
                var exception = exceptionBox.DispatchInfo?.SourceException;
                stack.Exception = exception;
            }
            else
            {
                // Want to barf out to exception handler here since the exception is unobserved
                // either due to being awaited (assume exception propagation down to stack root and handled there)
                // or has been forgotten.
                if (stack.HeadIndex == 1)
                {
                    Coroutine.OnUnhandledException?.Invoke(exceptionBox.DispatchInfo?.SourceException ?? new Exception("Coroutine faulted without exception information"));
                }
            }
        }

        stack.Pop();

        if (stack.HeadIndex > 0)
        {
            StackMoveNext(stack);
        }

        core.Free();
        coreToken.Return();

    end:
        // Restore the previous stack as the current active stack as we unwind
        // the StackMoveNext calls to ensure the correct execution context is maintained.
        CurrentStack = prevStack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool MoveAllNext()
    {
        List<Exception>? exceptions = null;

        for (CurrentIndex = 0; CurrentIndex < StackCount; CurrentIndex++)
        {
            var stack = Stacks[CurrentIndex];

            ProcessStack(stack, ref exceptions);
        }

        ref var currentContext = ref CurrentContext;
        currentContext.DeltaSeconds = 0;
        currentContext.StackCount = StackCount;

        if (exceptions != null)
        {
            HandleFaults(exceptions);
        }

        return StackCount != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void UpdateDelta(double delta)
    {
        if (!Coroutine.Clock.IsPositiveFinite(delta))
        {
            CoroutineClockException.ThrowClockUpdateMustBePositiveSeconds();
        }

        for (var i = 0; i < Contexts.Count; i++)
        {
            var context = Contexts[i];
            context.DeltaSeconds += delta;
            context.TotalElapsedSeconds += delta;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReturnStack(CoroutineStack stack)
    {
        DetachStack(stack);

        stack.StackToken.Return(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DetachStack(CoroutineStack stack)
    {
        if (stack.DispatcherIndex == -1)
        {
            return;
        }

        var moveIndex = StackCount - 1;

        Stacks[moveIndex].DispatcherIndex = stack.DispatcherIndex;
        Stacks[stack.DispatcherIndex] = Stacks[moveIndex];
        Stacks[moveIndex] = null!;
        stack.DispatcherIndex = -1;
        stack.Exception = null;
        stack.CoroutineContext = null!;
        StackCount--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineStack DetachActive()
    {
        if (CurrentStack == null)
        {
            throw new InvalidOperationException("No active stack to detach");
        }

        if (CurrentStack.DispatcherIndex != -1)
        {
            CurrentIndex--;
            DetachStack(CurrentStack);
        }

        return CurrentStack;
    }

    internal static CoroutineContext CreateContext()
    {
        CoroutineContext context;

        Contexts.Add(context = new CoroutineContext(new(), 0, 0, MainContext.TotalElapsedSeconds));

        return context;
    }

    internal static void SetContext(CoroutineContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        if (context.DispatcherId != Id)
        {
            throw new InvalidOperationException("Cannot set context from a different dispatcher");
        }

        if (CurrentContext != null)
        {
            CurrentContext.Stacks = Stacks;
            CurrentContext.StackCount = StackCount;
            CurrentContext.CurrentIndex = CurrentIndex;
            CurrentContext.DeltaSeconds = DeltaSeconds;
        }

        CurrentContext = context;
        Stacks = context.Stacks;
        StackCount = context.StackCount;
        CurrentIndex = context.CurrentIndex;
    }

    private static void HandleException(ref List<Exception>? exceptions, Exception ex)
    {
        exceptions ??= [];
        exceptions.Add(ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleFaults(List<Exception> exceptions)
    {
        Exception ex;

        if (exceptions.Count > 1)
        {
            ex = new AggregateException(exceptions);
        }
        else
        {
            ex = exceptions[0];
        }

        if (Coroutine.OnUnhandledException != null)
        {
            Coroutine.OnUnhandledException(ex);
        }
        else
        {
            throw ex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessStack(CoroutineStack stack, ref List<Exception>? exceptions)
    {
        StackMoveNext(stack);

        if (stack.HeadIndex == 0)
        {
            var exception = stack.Exception;

            ReturnStack(stack);
            CurrentIndex--;

            if (exception != null)
            {
                HandleException(ref exceptions, exception);
            }
        }
    }
}