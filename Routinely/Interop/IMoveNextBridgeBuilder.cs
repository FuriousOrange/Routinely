namespace Routinely.Interop;

internal interface IMoveNextBridgeBuilder
{
    CoroutineStateMachine Build<TStateMachine>(ref TStateMachine stateMachine) 
        where TStateMachine : IAsyncStateMachine;
}
