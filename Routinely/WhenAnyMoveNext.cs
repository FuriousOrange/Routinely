using Routinely.Interop;

namespace Routinely;

internal struct WhenAnyMoveNext<TCoroutine, TResult> : IMoveNextStateMachine
    where TCoroutine : struct, ICoroutine<TResult>
{
    private readonly ExchangeToken<WhenAnyContext<TCoroutine, TResult>> contextToken;
    private readonly int id;
    private readonly TCoroutine coroutine;

    public ExchangeToken<CoroutineCore> CoreToken { get; set; } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WhenAnyMoveNext(ExchangeToken<WhenAnyContext<TCoroutine, TResult>> contextToken, ref TCoroutine coroutine)
    {
        this.contextToken = contextToken;
        id = contextToken.Item.Id;
        this.coroutine = coroutine;
    }

    public readonly void MoveNext<TStateMachine>(ref TStateMachine stateMachine, ref CoroutineCore core)
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();

        ref var context = ref contextToken.Item;

        if (id != context.Id)
        {
            // Context has either faulted or completed
            return;
        }

        if (core.HasFlag(CoroutineCore.Completed) && context.Result == null)
        {
            // Clear awaited flag that was set during Coroutine.WhenAny so that the
            // coroutine can be accessed as the WhenAny result without faulting due to multiple awaits.
            core.ClearFlag(CoroutineCore.Awaited);
            context.Result = coroutine;
        }
    }
}
