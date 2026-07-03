using System.Diagnostics;

namespace Routinely.Policies;

public interface IThreadPolicy
{
    public static abstract bool ResumeAll();

    public static abstract CoroutineContext CreateContext();
}

public class ThreadIsolatedPolicy : IThreadPolicy
{
    public static bool ResumeAll() => StackDispatcher.MoveAllNext();

    public static CoroutineContext CreateContext() => StackDispatcher.CreateContext();
}

public class ThreadSharedPolicy : IThreadPolicy
{
    public static bool ResumeAll() => StackDispatcher.MoveAllNext();
    public static CoroutineContext CreateContext() => ThreadSharedBackplane.CreateContext();
}

public static class ThreadSharedBackplane
{
    private static readonly object backplaneLock = new();

    private static BackplaneChannelProvider[] channelProviders = new BackplaneChannelProvider[8];

    private static int providerCount = 0;

    public static CoroutineContext CreateContext()
    {
        lock (backplaneLock)
        {
            var currentDispatchedId = StackDispatcher.Id;

            while (currentDispatchedId >= channelProviders.Length)
            {
                Array.Resize(ref channelProviders, channelProviders.Length * 2);
            }

            var context = StackDispatcher.CreateContext();

            var backChannelProvider = channelProviders[currentDispatchedId] ??= new BackplaneChannelProvider();

            providerCount++;

            return context;
        }
    } 
}

public sealed class BackplaneChannelProvider()
{
    private BackplaneChannel[] channels = new BackplaneChannel[8];

    public void AddContext(CoroutineContext context)
    {

    }
}

public sealed class BackplaneChannel(CoroutineContext context)
{
}
