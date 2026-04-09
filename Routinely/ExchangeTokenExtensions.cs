namespace Routinely;

public static class ExchangeTokenExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref T UnsafeGet<T>(this ExchangeToken source)
        => ref Unsafe.As<ExchangeToken, ExchangeToken<T>>(ref source).Item;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe internal static void Return(this ExchangeToken source)
    { 
        var trampolines = Exchange.Trampolines[source.TypeId];
        trampolines.ReturnTrampoline(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TResult BindOnNull<TResult>(this ExchangeToken<TResult> source, Func<ExchangeToken<TResult>, TResult> factory)
        where TResult : class
        => source.Item ??= factory(source);
}
