namespace Routinely;

public sealed partial class CancellationContract : ICoroutineNotifyAwaited<CancellationContract>, IDisposable
{
    private ExchangeToken<CoroutineCore> coreToken = null!;

    private readonly ExchangeToken<CancellationContract> contractToken;

    public bool IsCompleted => false;

    public CoroutineStack Stack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    } = null!;

    public ExchangeToken<CoroutineCore> CoreToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => coreToken;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CancellationContract GetAwaiter() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CancellationContract GetResult() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CancellationContract(ExchangeToken<CancellationContract> contractToken)
    {
        this.contractToken = contractToken;
    }

    public void Enforce()
    {
        if (coreToken == null)
            throw new InvalidOperationException("Cancelation contract was not awaited.");

        if (coreToken.Item.HasFlag(CoroutineCore.Canceled))
        {
            throw new CancellationException(CoreToken.Item);
        }
    }

    public TCoroutine Coroutine<TCoroutine>(TCoroutine coroutine)
        where TCoroutine : struct, ICoroutine
    {
        Enforce();

        return coroutine;
    }

    void ICoroutineNotifyAwaiting.GetResult()
    {
    }

    void ICoroutineNotifyAwaiting.OnAwaiting<TCoroutine, TAwaiter, TStateMachine>(ref TCoroutine coroutine, ref TAwaiter awaiter, ref TStateMachine stateMachine)
    {
        if (coreToken != null)
            throw new InvalidOperationException("Cancelation contract was already awaited.");

        if (coroutine.IsCompleted)
        {
            coroutine.Configure(ref stateMachine, StackDispatcher.GetStack());
            Stack = coroutine.Stack;
        }
        else
        {
            Stack = StackDispatcher.CurrentStack;
        }

        coreToken = coroutine.CoreToken;
        ref var core = ref coreToken.Item;
        core.SetFlag(CoroutineCore.CancellationContract);

        Enforce();

        StackDispatcher.StackMoveNext(Stack);
    }

    public void Dispose()
    {
        var contract = contractToken.Item;
        contract.coreToken = null!;
        contractToken.Return(false);
    }
}
