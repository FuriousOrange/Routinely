using System.Diagnostics.CodeAnalysis;

namespace Routinely;

public static class CoroutineExtensions
{
    /// <summary>
    /// Used for fire and forget coroutines.
    /// </summary>
    /// <typeparam name="TCoroutine"></typeparam>
    /// <param name="coroutine"></param>
    /// <remarks>
    /// This method is used to indicate that the coroutine's result will not be awaited or captured.
    /// The coroutine's internal resources will be recycled immediately upon completion. Usually a coroutine's
    /// state is persisted for one tick after completion to allow user code to observe it's final state.
    /// <code>
    /// async Coroutine Work()
    /// {
    ///     // Logic...
    /// }
    /// 
    /// Work().Forget();
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget<TCoroutine>(this TCoroutine coroutine)
        where TCoroutine : struct, ICoroutine
    {
        if (coroutine.IsCompleted)
            return;

        coroutine.CoreToken.Item.SetFlag(CoroutineCore.Awaited);
    }

    /// <summary>
    /// Cancels a coroutine.
    /// </summary>
    /// <typeparam name="TCoroutine"></typeparam>
    /// <param name="coroutine"></param>
    /// <remarks>
    /// This method is used to cancel a coroutine. If the coroutine has already completed or been canceled, this method has no effect.
    /// If the coroutine has been awaited it will throw a <see cref="AwaitedCoroutineException"/> as an awaited coroutine has been merged
    /// into the callstack of its awaiter and cannot be safely canceled. Instead the root awaiter should be canceled to ensure all child coroutines are also canceled.
    /// 
    /// If a coroutine uses resources that require cleanup then it should use <see cref="CancellationContract.Enter"/>
    /// to define a cancellation contract. This will ensure that the coroutine observes cancellation requests and performs cleanup as necessary.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Cancel<TCoroutine>(this TCoroutine coroutine)
        where TCoroutine : struct, ICoroutine
    {
        if (!coroutine.HasContext)
        {
            return;
        }

        if (coroutine.IsCompleted)
        {
            return;
        }

        if (coroutine.IsCanceled)
        {
            return;
        }

        if (coroutine.IsAwaited)
        {
            AwaitedCoroutineException.ThrowInvalidCancellation(coroutine);
        }

        ref var cancellingCore = ref coroutine.CoreToken.Item;
        var stack = coroutine.Stack;

        ref var core = ref coroutine.CoreToken.Item;

        for (var i = stack.HeadIndex - 1; i >= 0; i--)
        {
            core = ref stack.Tokens[i].Item;
            core.Cancel();
        }
    }

    /// <summary>
    /// Set the current coroutine context from a previously created <see cref="CreateContext"/>.
    /// </summary>
    /// <param name="context">The context to set.</param>
    /// <typeparam name="TCoroutine"></typeparam>
    /// <param name="coroutine">The coroutine to set the context for.</param>
    /// <param name="context">The context to set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetContext<TCoroutine>(this TCoroutine coroutine, CoroutineContext context)
        where TCoroutine : struct, ICoroutine
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var stack = coroutine.Stack;
        var currentContext = stack.CoroutineContext;

        if(currentContext == context)
        {
            return;
        }

        if (currentContext == StackDispatcher.CurrentContext)
        {
            StackDispatcher.DetachForMigrate(stack);
        }
        else
        {
            currentContext.DetachStack(stack);
        }

        context.MigrateStack(stack);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNoContext<TCoroutine>(this TCoroutine coroutine)
        where TCoroutine : struct, ICoroutine
    {
        if (!coroutine.HasContext)
        {
            throw new NoContextException(coroutine);
        }
    }
}