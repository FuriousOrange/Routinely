using System.Runtime.InteropServices;

namespace Routinely;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[SkipLocalsInit]
public struct CoroutineCore
{
    [ThreadStatic]
    internal static uint CurrentId;

    internal const int
        Completed = 1 << 0,
        Faulted = 1 << 1,
        Canceled = 1 << 2,
        CancellationContract = 1 << 3,
        Awaited = 1 << 4,
        TailCall = 1 << 5;

    internal byte Flags;
    internal uint Id;
    internal byte DispatcherId;
    internal CoroutineStateMachine StateMachine;
    internal ExchangeToken? Result;

    public readonly bool HasContext => Id != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasFlag(byte flag) => (Flags & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFlag(byte flag) => Flags |= flag;

    public void ClearFlag(byte flag) => Flags &= (byte)~flag;

    public override readonly string ToString()
    {
        return $"Completed: {this.HasFlag(Completed)}, Faulted: {this.HasFlag(Faulted)}, Canceled: {this.HasFlag(Canceled)}, CancelationContract: {this.HasFlag(CancellationContract)}, Awaited: {this.HasFlag(Awaited)}, TailCall: {this.HasFlag(TailCall)}, Id: {Id}";
    }
}
