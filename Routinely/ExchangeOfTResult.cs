namespace Routinely;

internal sealed class Exchange<T> : Exchange
{
    [ThreadStatic]
    static ExchangeToken<T> Head;

    [ThreadStatic]
    internal static int TypeId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Exchange()
    {
        if (TypeIdCounter >= Trampolines.Length)
        {
            Array.Resize(ref Trampolines, Trampolines.Length * 2);
        }

        Trampolines[TypeId = TypeIdCounter++] = new ExchangeTrampolines()
        {
            ReturnTrampoline = &Exchange<T>.ReturnTrampoline
        };

        Head = new ExchangeToken<T>(TypeId, null!, default!);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExchangeToken<T> Reserve()
    {
        Head ??= new ExchangeToken<T>(TypeId, null!, default!);

        var index = Head.Free;

        if (index != null)
        {
            Head.Free = index.Free;

            var replace = index;
            replace.Free = null!;

            return replace!;
        }

        var fresh = new ExchangeToken<T>(TypeId, Head, default!);

        return fresh;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ExchangeToken<T> Put(T item)
    {
        Head ??= new ExchangeToken<T>(TypeId, null!, default!);

        var index = Head.Free;

        if (index != null)
        {
            Head.Free = index.Free;
            var replace = index;
            replace.Item = item;

            return replace;
        }

        var fresh = new ExchangeToken<T>(TypeId, Head, item);

        return fresh;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReturnTrampoline(ExchangeToken token)
    {
        ref var typedToken = ref Unsafe.As<ExchangeToken, ExchangeToken<T>>(ref token);
        typedToken.Return();
    }
}