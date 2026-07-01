namespace Routinely.Interop;

public static class CustomStateMachineExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Bind<TCustomStateMachine>(ref this TCustomStateMachine stateMachine, ref CoroutineCore core)
        where TCustomStateMachine : struct, ICustomStateMachine
        => core.StateMachine = CoroutineStateMachine<TCustomStateMachine>.CaptureStateMachine(ref stateMachine);
}
