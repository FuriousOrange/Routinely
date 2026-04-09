namespace Routinely.Interop;

internal sealed class MoveNextBridgeBuilder<TMoveNextStateMachine> : IMoveNextBridgeBuilder
    where TMoveNextStateMachine : IMoveNextStateMachine
{
    [ThreadStatic]
    unsafe private static void* stateMachinePtr;

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
    unsafe internal static MoveNextBridgeBuilder<TMoveNextStateMachine> Start(ref TMoveNextStateMachine moveNextStateMachine, ExchangeToken<CoroutineCore> coreToken)
    {
        stateMachinePtr = Unsafe.AsPointer(ref moveNextStateMachine);
        MoveNextBridgeBuilder<TMoveNextStateMachine>.coreToken = coreToken;
        return Instance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe CoroutineStateMachine IMoveNextBridgeBuilder.Build<TStateMachine>(ref TStateMachine stateMachine) 
    {
        var moveNextBridge = new MoveNextBridge<TMoveNextStateMachine, TStateMachine>(ref stateMachine, ref Unsafe.AsRef<TMoveNextStateMachine>(stateMachinePtr), coreToken!);
        return CoroutineStateMachine<MoveNextBridge<TMoveNextStateMachine, TStateMachine>>.CaptureStateMachine(ref moveNextBridge);
    }
}
