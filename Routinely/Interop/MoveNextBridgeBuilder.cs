namespace Routinely.Interop;

internal sealed class MoveNextBridgeBuilder<TMoveNextStateMachine> : IMoveNextBridgeBuilder
    where TMoveNextStateMachine : struct, IMoveNextStateMachine
{
    [ThreadStatic]
    private static TMoveNextStateMachine stateMachine;

    [ThreadStatic]
    private static ExchangeToken<CoroutineCore>? coreToken;

    [ThreadStatic]
    private static MoveNextBridgeBuilder<TMoveNextStateMachine>? instance;

    internal static MoveNextBridgeBuilder<TMoveNextStateMachine> Instance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => instance ??= new MoveNextBridgeBuilder<TMoveNextStateMachine>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static MoveNextBridgeBuilder<TMoveNextStateMachine> Start(ref TMoveNextStateMachine moveNextStateMachine, ExchangeToken<CoroutineCore> coreToken)
    {
        stateMachine = moveNextStateMachine;
        MoveNextBridgeBuilder<TMoveNextStateMachine>.coreToken = coreToken;
        return Instance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    CoroutineStateMachine IMoveNextBridgeBuilder.Build<TStateMachine>(ref TStateMachine stateMachine) 
    {
        var moveNextBridge = new MoveNextBridge<TMoveNextStateMachine, TStateMachine>(ref stateMachine, ref MoveNextBridgeBuilder<TMoveNextStateMachine>.stateMachine, coreToken!);
        coreToken = null;
        return CoroutineStateMachine<MoveNextBridge<TMoveNextStateMachine, TStateMachine>>.CaptureStateMachine(ref moveNextBridge);
    }
}
