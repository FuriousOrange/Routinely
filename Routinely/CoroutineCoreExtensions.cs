using System.Runtime.ExceptionServices;

namespace Routinely;

public static class CoroutineCoreExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Cancel(ref this CoroutineCore core)
    {
        core.SetFlag(CoroutineCore.Completed | CoroutineCore.Canceled);

        if (core.HasFlag(CoroutineCore.CancellationContract))
        {
            try
            {
                core.StateMachine.MoveNext();
            }
            catch (CancellationException)
            {
                // Want to swallow this exception since it is expected to be thrown by the state machine when it observes the cancellation contract flag.
                // The state machine will have already transitioned to the completed and cancelled state, so we don't need to do anything else here.
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Fault(ref this CoroutineCore core, Exception ex)
    {
        var exceptionBox = new ExceptionBox(ExceptionDispatchInfo.Capture(ex));

        core.Result = Exchange<ExceptionBox>.Put(exceptionBox);

        core.SetFlag(CoroutineCore.Faulted | CoroutineCore.Completed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Fault(ref this CoroutineCore core, ExceptionBox exceptionBox)
    {
        core.Result = Exchange<ExceptionBox>.Put(exceptionBox);

        core.SetFlag(CoroutineCore.Faulted | CoroutineCore.Completed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Throw(ref this CoroutineCore core)
    {
        ExceptionBox exceptionBox = default;
        core.Result?.CheckedGet(ref exceptionBox);

        exceptionBox.DispatchInfo?.Throw();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetResult<TResult>(ref this CoroutineCore core, TResult result)
    => core.Result = Exchange<TResult>.Put(result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Free(ref this CoroutineCore core)
    {
        core.StateMachine.Free();
        core.Result?.Return();
    }
}