namespace Routinely.Interop;

internal struct MoveNextBridge<TMoveNextStateMachine, TStateMachine> : IAsyncStateMachine
    where TMoveNextStateMachine : IMoveNextStateMachine
    where TStateMachine : IAsyncStateMachine
{
    internal TStateMachine stateMachine;
    internal readonly TMoveNextStateMachine moveNextStateMachine;
    internal readonly ExchangeToken<CoroutineCore> coreToken;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal MoveNextBridge(ref TStateMachine stateMachine, ref TMoveNextStateMachine moveNextStateMachine, ExchangeToken<CoroutineCore> coreToken)
    {
        this.stateMachine = stateMachine;
        this.moveNextStateMachine = moveNextStateMachine;
        this.coreToken = coreToken;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IAsyncStateMachine.MoveNext()
    {
        moveNextStateMachine.MoveNext(ref stateMachine, ref coreToken.Item);
    }

    readonly void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine) { }
}
