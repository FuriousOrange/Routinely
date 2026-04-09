using System.Runtime.ExceptionServices;

namespace Routinely;

internal readonly struct ExceptionBox(ExceptionDispatchInfo dispatchInfo)
{
    internal readonly ExceptionDispatchInfo DispatchInfo = dispatchInfo;
}
