using System.Diagnostics.CodeAnalysis;

namespace Routinely;

public partial struct Coroutine : 
    IEquatable<Coroutine>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Coroutine other)
        => Id == other.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Coroutine a, Coroutine b)
    {
        return a.Equals(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Coroutine a, Coroutine b)
    {
        return !a.Equals(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is ICoroutine coroutine)
        {
            return Id == coroutine.Id;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode()
        => base.GetHashCode();
}