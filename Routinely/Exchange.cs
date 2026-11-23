namespace Routinely;

internal class Exchange
{
    protected static readonly object Lock = new();

    internal readonly struct ExchangeTrampolines
    {
        internal unsafe delegate*<ExchangeToken, void> ReturnTrampoline
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init;
        }
    }

    internal static int TypeIdCounter;

    internal static ExchangeTrampolines[] Trampolines;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Exchange()
    {
        Trampolines = new ExchangeTrampolines[256];
    }
}
