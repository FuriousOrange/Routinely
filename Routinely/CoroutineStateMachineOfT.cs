using Routinely.Interop;

namespace Routinely;

internal readonly struct CoroutineStateMachine<TStateMachine>
    where TStateMachine : IAsyncStateMachine
{
    [ThreadStatic]
    internal static int TypeId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static CoroutineStateMachine()
    {
        while (CoroutineStateMachine.IdCounter >= CoroutineStateMachine.Trampolines.Length)
        {
            Array.Resize(ref CoroutineStateMachine.Trampolines, CoroutineStateMachine.Trampolines.Length * 2);
        }

        CoroutineStateMachine.Trampolines[TypeId = CoroutineStateMachine.IdCounter++] = new CoroutineStateMachine.ContinuationTrampolines
        {
            MoveNext = &MoveNextTrampoline,
            Bridge = &BridgeTrampoline,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineStateMachine CaptureStateMachine(ref TStateMachine target)
    {
        var stateMachineToken = Exchange<TStateMachine>.Put(target);

        return new CoroutineStateMachine()
        {
            Id = TypeId,
            StateMachineToken = stateMachineToken,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Update(CoroutineStateMachine stateMachineToken, ref TStateMachine stateMachine)
    {

        var exchangeToken = Unsafe.As<ExchangeToken, ExchangeToken<TStateMachine>>(ref stateMachineToken.StateMachineToken);
        exchangeToken.Item = stateMachine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MoveNextTrampoline(ExchangeToken stateMachineToken)
        => Unsafe.As<ExchangeToken, ExchangeToken<TStateMachine>>(ref stateMachineToken).Item.MoveNext();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CoroutineStateMachine BridgeTrampoline(ref IMoveNextBridgeBuilder bridgeBuilder, in CoroutineStateMachine stateMachine)
    {
        var stateMachineToken = stateMachine.StateMachineToken;
        var exchangeToken = Unsafe.As<ExchangeToken, ExchangeToken<TStateMachine>>(ref stateMachineToken);

        var moveNextStateMachine = bridgeBuilder.Build(ref exchangeToken.Item);
        stateMachine.Free();

        return moveNextStateMachine;
    }
}
