using System.Diagnostics;

namespace Routinely;

public class CancellationException(CoroutineCore core) : Exception(CancellationMessage)
{
    public const string CancellationMessage = "Coroutine was canceled. See the CanceledCoroutineId for details.";

    public uint CancelledCoroutineId { get; } = core.Id;

    public StackTrace? CapturedStackTrace { get; } = new StackTrace(skipFrames: 1, fNeedFileInfo: true);

    public CancellationException(ICoroutine canceledCoroutine) 
        : this(canceledCoroutine.CoreToken.Item)
    {
    }

    public override string? StackTrace => CapturedStackTrace?.ToString();
}