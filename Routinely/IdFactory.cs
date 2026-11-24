namespace Routinely;

public static class IdFactory<T>
{
    internal const uint Uninitialized = 0;

    [ThreadStatic]
    internal static uint CurrentId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetNext()
    {
        // Id zero is reserved for uninitialized,
        // so we start from 1 and skip 0 if we end up wrapping around
        unchecked
        {
            if (++CurrentId == 0)
                CurrentId = 1;

            return CurrentId;
        }
    }
}
