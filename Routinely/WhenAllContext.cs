namespace Routinely;

internal struct WhenAllContext<TCoroutine, TResult>(TCoroutine[] coroutines, TResult[]? results)
    where TCoroutine: struct, ICoroutine<TResult>
{
    [ThreadStatic]
    private static int CurrentId;

    internal int Count;
    internal readonly TCoroutine[] Coroutines = coroutines;
    internal TResult[]? Results = results;

    internal readonly int Id = ++CurrentId;
    internal List<Exception>? Exceptions;
}