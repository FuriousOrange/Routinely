using System.Diagnostics.CodeAnalysis;

namespace Routinely;

public class AwaitedCoroutineException(string message, ICoroutine awaitedCoroutine) : Exception(message)
{
    public const string AwaitMessage = "A coroutine cannot be awaited multiple times. See the AwaitedCoroutine for details.";

    public const string InvalidCancellationMessage = "A coroutine that was awaited cannot be canceled. See the AwaitedCoroutine for details.";

    public const string AwaitedTaskAsCoroutineCancellationMessage = "A task as a coroutine was canceled.";

    public ICoroutine AwaitedCoroutine { get; } = awaitedCoroutine;

    [DoesNotReturn]
    public static void ThrowMultipleAwait(ICoroutine awaitedCoroutine)
    {
        throw new AwaitedCoroutineException(AwaitMessage, awaitedCoroutine);
    }

    [DoesNotReturn]
    public static AwaitedCoroutineException ThrowInvalidCancellation(ICoroutine awaitedCoroutine)
    {
        throw new AwaitedCoroutineException(InvalidCancellationMessage, awaitedCoroutine);
    }

    [DoesNotReturn]
    public static AwaitedCoroutineException ThrowAwaitedTaskAsCoroutineCancellation(ICoroutine awaitedCoroutine)
    {
        throw new AwaitedCoroutineException(AwaitedTaskAsCoroutineCancellationMessage, awaitedCoroutine);
    }
}
