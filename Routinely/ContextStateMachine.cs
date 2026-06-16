using Routinely.Interop;

namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct ContextStateMachine(CoroutineContext context) : ICustomStateMachine
{
    private readonly CoroutineContext context = context;
    private bool started = false;

    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    public void ConfigureCoreToken(ExchangeToken<CoroutineCore> token)
    {
        CoreToken = token;
        ref var core = ref CoreToken.Item;
        core.SetFlag(CoroutineCore.Awaited);
        core.StateMachine = CoroutineStateMachine<ContextStateMachine>.CaptureStateMachine(ref this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveNext()
    {
        if (!started)
        {
            var stack = StackDispatcher.DetachActive();
            context.MigrateStack(stack);
            started = true;
            return;
        }
        else
        {
            CoreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }
}