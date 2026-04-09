namespace Routinely.Interop;

public interface IMoveNextStateMachine

{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void MoveNext<TStateMachine>(ref TStateMachine stateMachine, ref CoroutineCore core)
        where TStateMachine : IAsyncStateMachine;
}
