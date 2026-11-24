namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal struct WhenAnyContext<TCoroutine, TResult>(TCoroutine[] coroutines)
    where TCoroutine: struct, ICoroutine<TResult>
{
    internal readonly uint Id = IdFactory<WhenAnyContext<TCoroutine, TResult>>.GetNext();

    internal readonly TCoroutine[] Coroutines = coroutines;

    internal TCoroutine? Result;

    public readonly bool HasContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id != IdFactory<WhenAnyContext<TCoroutine, TResult>>.Uninitialized;
    }
}
