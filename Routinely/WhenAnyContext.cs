namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct WhenAnyContext<TCoroutine, TResult>(TCoroutine[] coroutines)
    where TCoroutine: struct, ICoroutine<TResult>
{
    [ThreadStatic]
    private static int CurrentId;

    internal readonly int Id = ++CurrentId;

    internal readonly TCoroutine[] Coroutines = coroutines;

    internal TCoroutine? Result;
}
