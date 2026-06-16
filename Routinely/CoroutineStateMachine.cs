using Routinely.Interop;

namespace Routinely;

internal struct CoroutineStateMachine
{
    internal static readonly object Lock = new();

    internal readonly struct ContinuationTrampolines
    {
        unsafe internal delegate*<ExchangeToken, void> MoveNext 
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init; 
        }

        unsafe internal delegate*<ref IMoveNextBridgeBuilder, in CoroutineStateMachine, CoroutineStateMachine> Bridge 
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init; 
        }
    }

    //[ThreadStatic]
    internal static int IdCounter;

    //[ThreadStatic]
    internal static ContinuationTrampolines[] Trampolines;

    internal int Id;

    internal ExchangeToken StateMachineToken;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static CoroutineStateMachine()
    {
        Trampolines = new ContinuationTrampolines[2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly unsafe void MoveNext()
        => Trampolines[Id].MoveNext(StateMachineToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe internal readonly CoroutineStateMachine Bridge(ref IMoveNextBridgeBuilder bridgeEnd)
        => Trampolines[Id].Bridge(ref bridgeEnd, in this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly void Free() => StateMachineToken.Return();
}
