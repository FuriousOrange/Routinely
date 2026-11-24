namespace Routinely;

internal struct WhenAllContext<TCoroutine, TResult>(TCoroutine[] coroutines, TResult[]? results)
    where TCoroutine : struct, ICoroutine<TResult>
{
    internal int Count;
    internal readonly TCoroutine[] Coroutines = coroutines;
    internal TResult[]? Results = results;

    internal readonly uint Id = IdFactory<WhenAllContext<TCoroutine, TResult>>.GetNext();
    internal List<Exception>? Exceptions;

    public readonly bool HasContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id != IdFactory<WhenAllContext<TCoroutine, TResult>>.Uninitialized;
    }
}