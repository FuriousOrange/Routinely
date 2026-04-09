using System.Diagnostics.CodeAnalysis;

namespace Routinely;

public partial struct Coroutine<TResult> : 
    IEquatable<Coroutine<TResult>>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Coroutine<TResult> other) => Id == other.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Coroutine<TResult> a, Coroutine<TResult> b) => a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Coroutine<TResult> a, Coroutine<TResult> b) => !a.Equals(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine(Coroutine<TResult> coroutine) => new(coroutine.Id)
    {
        CoreToken = coroutine.CoreToken,
        Stack = coroutine.Stack,
        isCompleted = coroutine.IsCompleted,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is ICoroutine coroutine)
        {
            return Id == coroutine.Id;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode()
        => base.GetHashCode();
}
