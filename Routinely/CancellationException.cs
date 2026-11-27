using System.Diagnostics;

namespace Routinely;

public class CancellationException(CoroutineCore core) : Exception(CancellationMessage)
{
    internal const string CancellationMessage = "Coroutine was canceled. See the CanceledCoroutineId for details.";

    internal uint CancelledCoroutineId { get; } = core.Id;

    internal StackTrace? CapturedStackTrace { get; } = new StackTrace(skipFrames: 1, fNeedFileInfo: true);

    internal CancellationException(ICoroutine canceledCoroutine) 
        : this(canceledCoroutine.CoreToken.Item)
    {
    }

    public override string? StackTrace => CapturedStackTrace?.ToString();
}
