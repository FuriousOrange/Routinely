using Routinely.Interop;

namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct WhenAllMoveNext<TCoroutine, TResult>(int index, ExchangeToken<WhenAllContext<TCoroutine, TResult>> contextToken) : IMoveNextStateMachine
    where TCoroutine : struct, ICoroutine<TResult>
{
    private readonly int index = index;
    private readonly ExchangeToken<WhenAllContext<TCoroutine, TResult>> contextToken = contextToken;
    private readonly int id = contextToken.Item.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext<TStateMachine>(ref TStateMachine stateMachine, ref CoroutineCore core)
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();

        ref var context = ref contextToken.Item;

        if (id != context.Id)
        {
            // Context has either faulted or completed
            return;
        }

        if (core.HasFlag(CoroutineCore.Completed))
        {
            if (context.Results != null)
            {
                TResult result = default!;
                core.Result?.CheckedGet(ref result);
                context.Results[index] = result;
            }

            if (core.HasFlag(CoroutineCore.Faulted))
            {
                ExceptionBox exceptionBox = default!;
                core.Result?.CheckedGet(ref exceptionBox);

                if (exceptionBox.DispatchInfo != null)
                {
                    context.Exceptions ??= [];
                    context.Exceptions.Add(exceptionBox.DispatchInfo.SourceException);
                }
            }

            context.Count--;
        }
    }
}