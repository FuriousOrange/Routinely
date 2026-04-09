namespace Routinely.Interop;

public interface ICustomStateMachine : IAsyncStateMachine
{
    ExchangeToken<CoroutineCore> CoreToken { get; set; }

    void ConfigureCoreToken(ExchangeToken<CoroutineCore> token);

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}
