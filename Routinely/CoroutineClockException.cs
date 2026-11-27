using System.Diagnostics.CodeAnalysis;

namespace Routinely;

public class CoroutineClockException(string message) : Exception(message)
{
    internal const string ClockNotUpdatedMessage = "Coroutine clock was not updated during the yield. Ensure that Coroutine.Clock.Update() is called in the main loop.";
    
    internal const string ClockUpdatePositiveFiniteMessage = "Coroutine clock must be updated with a positive finite value.";
    
    internal const string DelayUpdateZeroOrPositiveFiniteMessage = "Coroutine delay must be called with 0 or a positive finite value.";

    [DoesNotReturn]
    internal static void ThrowClockNotUpdated()
    {
        throw new CoroutineClockException(ClockNotUpdatedMessage);
    }

    [DoesNotReturn]
    internal static void ThrowClockUpdateMustBePositiveSeconds()
    {
        throw new CoroutineClockException(ClockUpdatePositiveFiniteMessage);
    }

    [DoesNotReturn]
    internal static void ThrowDelayMustBeZeroOrPositiveSeconds()
    {
        throw new CoroutineClockException(DelayUpdateZeroOrPositiveFiniteMessage);
    }
}