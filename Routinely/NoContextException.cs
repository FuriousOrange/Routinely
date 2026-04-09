namespace Routinely;

public class NoContextException : Exception
{
    public const string LostContextMessage = "Coroutine does not have context with the coroutine dispatcher.";
    
    public NoContextException() : base(LostContextMessage)
    {
    }
}
