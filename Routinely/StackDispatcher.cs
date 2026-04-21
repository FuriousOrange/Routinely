using System.Diagnostics;

namespace Routinely;

internal static class StackDispatcher
{
    [ThreadStatic]
    internal static int StackCount;

    [ThreadStatic]
    internal static PartitionArray<CoroutineStack> stacks;

    [ThreadStatic]
    internal static List<CoroutineContext> Contexts;

    [ThreadStatic]
    internal static CoroutineContext? CurrentContext;

    [ThreadStatic]
    internal static CoroutineStack CurrentStack;

    [ThreadStatic]
    internal static int currentIndex;

    static StackDispatcher()
    {
        stacks = new();
        CurrentStack = null!;
        Contexts = new();
        CurrentContext = new(stacks, StackCount, currentIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineStack GetStack()
    {
        var stack = CoroutineStack.Get();

        if (StackCount >= stacks.Length)
        {
            stacks.Expand();
        }

        stack.DispatcherIndex = StackCount;
        stack.CoroutineContext = CurrentContext!;
        stacks[StackCount++] = stack;

        return stack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExchangeToken<CoroutineCore> GetCoroutineCoreToken()
    {
        var core = new CoroutineCore();
        var token = Exchange<CoroutineCore>.Put(core);
        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void MergeActive(CoroutineStack awaitStack)
    {
        CurrentStack.MergeStack(awaitStack);
        ReturnStack(awaitStack);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StackMoveNext(CoroutineStack stack)
    {
        Debug.Assert(stack != null, "StackMoveNext called with null stackToken");

        // Need to capture the current active stack here as the previous stack
        // to ensure that when we recursively call StackMoveNext
        // we don't lose the stack execution context of the current stack.
        var prevStack = CurrentStack;
        CurrentStack = stack;

        Debug.Assert(stack != null, "StackMoveNext called with null stack");

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

            if (!core.HasFlag(CoroutineCore.Awaited))
            {
                var exception = exceptionBox.DispatchInfo?.SourceException;
                stack.Exception = exception;
            }
            else if (core.HasFlag(CoroutineCore.TailCall))
            {
                stack.Tokens[0].Item.Fault(exceptionBox);
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

        for (currentIndex = 0; currentIndex < StackCount; currentIndex++)
        {
            var stack = stacks[currentIndex];

            ProcessStack(stack, ref exceptions);
        }

        if (exceptions != null)
        {
            HandleFaults(exceptions);
        }

        return StackCount != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReturnStack(CoroutineStack stack)
    {
        Debug.Assert(stack != null, "ReturnStack called with null stack");
        Debug.Assert(stack.StackToken != null, "ReturnStack called with null stackToken");
        Debug.Assert(stack.HeadIndex == 0, "ReturnStack called on non-empty stack");

        DetachStack(stack);

        stack.StackToken.Return(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DetachStack(CoroutineStack stack)
    {
        var moveIndex = StackCount - 1;

        stacks[moveIndex].DispatcherIndex = stack.DispatcherIndex;
        stacks[stack.DispatcherIndex] = stacks[moveIndex];
        stacks[moveIndex] = null!;
        stack.Exception = null;
        stack.CoroutineContext = null!;
        StackCount--;
    }

    internal static CoroutineContext CreateContext()
    {
        CoroutineContext context;

        if (Contexts.Count == 0)
        {
            Contexts.Add(context = CurrentContext!);
        }
        else
        {
            Contexts.Add(context = new CoroutineContext(new(), 0, 0));
        }

        return context;
    }

    internal static void SetContext(CoroutineContext context)
    {
        if(CurrentContext != null)
        {
            CurrentContext.Stacks = stacks;
            CurrentContext.StackCount = StackCount;
            CurrentContext.CurrentIndex = currentIndex;
        }

        CurrentContext = context;
        stacks = context.Stacks;
        StackCount = context.StackCount;
        currentIndex = context.CurrentIndex;
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

            ReturnStack(stacks[currentIndex]);
            currentIndex--;

            if (exception != null)
            {
                HandleException(ref exceptions, exception);
            }
        }
    }
}