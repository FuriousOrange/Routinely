using System.Runtime.ExceptionServices;

namespace Routinely;

public interface ICoroutineMethodBuilder<TCoroutine, TResult>
    where TCoroutine : struct, ICoroutine<TResult>
{
    void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine;

    void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine;

    void SetException(Exception ex);

    void SetStateMachine(IAsyncStateMachine stateMachine);

    void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OnAwait<TMethodBuilder, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICoroutineNotifyAwaiting
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnAwaiting(ref coroutine, ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetResultVoid(Coroutine coroutine)
    {
        if(coroutine.IsCompleted)
        {
            return;
        }
        else
        {
            coroutine.CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetResult(ref Coroutine<TResult> coroutine, ref TResult result)
    {
        if (coroutine.IsCompleted)
        {
            coroutine.result = result;
            coroutine.isCompleted = true;
        }
        else
        {
            ref var core = ref coroutine.CoreToken.Item;

            if (core.Result != null)
            {
                return;
            }

            core.Result = Exchange<TResult>.Put(result);
            coroutine.result = result;
            coroutine.isCompleted = true;

            core.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetException(in TCoroutine coroutine, Exception ex)
    {
        // Fault a sync completion
        if (coroutine.IsCompleted)
        {
            ExceptionDispatchInfo.Throw(ex);
        }
        else
        {
            coroutine.CoreToken.Item.Fault(ex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StartInternal<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }
}