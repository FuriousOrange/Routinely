namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct ContextStateMachine(CoroutineContext context, ExchangeToken<CoroutineCore> coreToken) : IAsyncStateMachine
{
    private readonly CoroutineContext context = context;
    private readonly ExchangeToken<CoroutineCore> coreToken = coreToken;
    private bool started = false;

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
            coreToken.Item.SetFlag(CoroutineCore.Completed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}