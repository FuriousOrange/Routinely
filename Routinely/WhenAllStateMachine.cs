using Routinely.Interop;

namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct WhenAllStateMachine<TCoroutine, TResult>(ExchangeToken<WhenAllContext<TCoroutine, TResult>> contextToken) : ICustomStateMachine
    where TCoroutine : struct, ICoroutine<TResult>
{
    private readonly ExchangeToken<WhenAllContext<TCoroutine, TResult>> contextToken = contextToken;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void MoveNext()
    {
        ref var context = ref contextToken.Item;

        ref var core = ref CoreToken.Item;

        if (core.HasFlag(CoroutineCore.Canceled))
        {
            for (var i = 0; i < context.Coroutines.Length; i++)
            {
                var coroutine = context.Coroutines[i];

                // Need to clear awaited flag that was set during intial Coroutine.WhenAll
                // otherwise the cancel call will fault
                coroutine.CoreToken.Item.ClearFlag(CoroutineCore.Awaited);
                context.Coroutines[i].Cancel();
            }

            contextToken.Return();
            return;
        }

        if (context.Count == 0)
        {
            if (context.Results != null && context.Exceptions == null)
            {
                core.SetResult(context.Results);
            }

            core.SetFlag(CoroutineCore.Completed);

            try
            {
                if (context.Exceptions != null && context.Exceptions.Count > 0)
                {
                    throw new AggregateException(context.Exceptions);
                }
            }
            catch (Exception ex)
            {
                core.Fault(ex);
            }
            finally
            {
                contextToken.Return();
            }
        }
    }
}