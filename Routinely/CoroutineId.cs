namespace Routinely;

public static class CoroutineId
{
    [ThreadStatic]
    internal static uint CurrentId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetNextId()
    {
        // Id zero is reserved for coroutine cores that are uninitialized,
        // so we start from 1 and skip 0 if we exceed uint.MaxValue
        return ++CurrentId > 0 ? CurrentId : ++CurrentId;
    }
}
