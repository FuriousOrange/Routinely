namespace Routinely;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public class CoroutineStack(ExchangeToken<CoroutineStack> stackToken)
{
    internal readonly ExchangeToken<CoroutineStack> StackToken = stackToken;
    internal int HeadIndex;
    internal ExchangeToken<CoroutineCore>[] Tokens = new ExchangeToken<CoroutineCore>[8]; 
    internal int DispatcherIndex;
    internal Exception? Exception;
    internal CoroutineContext CoroutineContext = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Push(ExchangeToken<CoroutineCore> token)
    {
        while (HeadIndex >= Tokens.Length)
        {
            Array.Resize(ref Tokens, Tokens.Length * 2);
        }

        Tokens[HeadIndex++] = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Bump(ExchangeToken<CoroutineCore> token)
    {
        HeadIndex++;

        while (HeadIndex >= Tokens.Length)
        {
            Array.Resize(ref Tokens, Tokens.Length * 2);
        }

        if (HeadIndex != 0)
        {
            Tokens.AsSpan(0, HeadIndex).CopyTo(Tokens.AsSpan(1));
        }

        Tokens[0] = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Pop() => Tokens[--HeadIndex] = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExchangeToken<CoroutineCore> Peek() => Tokens[HeadIndex - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void MergeStack(CoroutineStack other)
    {
        if (other == null)
            return;

        var length = other.HeadIndex;

        for (var i = 0; i < length; i++)
        {
            Push(other.Tokens[i]);
            other.Tokens[i] = null!;
        }

        other.HeadIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void MigrateStack(CoroutineStack target)
    {
        MergeStack(target);

        var migrate = target.Tokens;

        target.Tokens = Tokens;
        Tokens = migrate;

        target.HeadIndex = HeadIndex;
        HeadIndex = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineStack Get()
        => Exchange<CoroutineStack>
            .Reserve()
            .BindOnNull(static token => new(token));
}
