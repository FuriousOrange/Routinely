namespace Routinely;

public class NoContextException : Exception
{
    public const string LostContextMessage = "Coroutine does not have context with the coroutine dispatcher.";
    
    public NoContextException(ICoroutine coroutine) : base(EnrichMessage(coroutine))
    {
    }

    private static string EnrichMessage(ICoroutine coroutine)
    {
        ref var core = ref coroutine.CoreToken.Item;

        if(core.Id != 0 && StackDispatcher.Id != core.DispatcherId)
        {
            return $"{LostContextMessage} Coroutine is associated with dispatcher {core.DispatcherId}. Current dispatcher is {StackDispatcher.Id}.";
        }

        if(core.Id == 0)
        {
            return $"{LostContextMessage} Coroutine has been recycled.";
        }

        return LostContextMessage;
    }
}
