namespace Routinely;

public static class CoroutineContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNoDispatcherAffinity(this CoroutineContext context)
    {
        if (context.DispatcherId != StackDispatcher.Id)
        {
            throw new InvalidOperationException("Coroutine context does not have dispatcher affinity");
        }
    }
}