namespace Routinely;

public interface ICoroutine : ICoroutineNotifyAwaiting
{
    uint Id { get; }

    bool HasContext { get; }

    bool IsFaulted { get; }

    bool IsCanceled { get; }

    bool CancelationContract { get; }

    bool IsAwaited { get; }

    Exception? Exception { get; }

    ExchangeToken<CoroutineCore> CoreToken { get; }

    void Configure<TStateMachine>(ref TStateMachine stateMachine, CoroutineStack stack)
        where TStateMachine : IAsyncStateMachine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Contextful(ExchangeToken<CoroutineCore> token, uint id)
        => token.Item.Id == id && token.Item.DispatcherId == StackDispatcher.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlag(ExchangeToken<CoroutineCore> token, byte flag, uint id)
    {
        ref var core = ref token.Item;

        if(core.Id != id)
        {
            return false;
        }

        return core.HasFlag(flag);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetFlag(in ExchangeToken<CoroutineCore> token, byte flag) => token.Item.Flags |= flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void UnsetFlag(in ExchangeToken<CoroutineCore> token, byte flag) => token.Item.Flags &= (byte)~flag;
}

public interface ICoroutine<TResult> : ICoroutine
{
    TResult? Result { get; }
}