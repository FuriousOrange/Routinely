using Routinely.Interop;

namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct WhenAnyStateMachine<TCoroutine, TResult>(ExchangeToken<WhenAnyContext<TCoroutine, TResult>> contextToken) : ICustomStateMachine
    where TCoroutine : struct, ICoroutine<TResult>
{
    private readonly ExchangeToken<WhenAnyContext<TCoroutine, TResult>> contextToken = contextToken;

    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ConfigureCoreToken(ExchangeToken<CoroutineCore> coreToken)
    {
        CoreToken = coreToken;
        coreToken.Item.SetFlag(CoroutineCore.CancellationContract);
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void MoveNext()
    {
        ref var context = ref contextToken.Item;

        ref var core = ref CoreToken.Item;

        if (core.HasFlag(CoroutineCore.Canceled))
        {
            for (var i = 0; i < context.Coroutines.Length; i++)
            {
                var coroutine = context.Coroutines[i];

                // Need to clear awaited that was set during intial Coroutine.WhenAny
                // otherwise the cancel call will fault
                coroutine.CoreToken.Item.ClearFlag(CoroutineCore.Awaited);
                context.Coroutines[i].Cancel();
            }
            contextToken.Return();
            return;
        }

        if (context.Result != null)
        {
            core.SetResult(context.Result.Value);
            core.SetFlag(CoroutineCore.Completed);
            contextToken.Return();
        }
    }
}
