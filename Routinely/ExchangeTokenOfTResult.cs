namespace Routinely;

public class ExchangeToken<TResult> : ExchangeToken
{
    internal ExchangeToken<TResult> Free = null!;
    
    internal TResult Item = default!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExchangeToken(int typeId, TResult item) : base(typeId)
    {
        Item = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Return(bool clearItem = true)
    {
        //Exchange<TResult>.Head ??= new ExchangeToken<TResult>(TypeId, default!);

        var currentFree = Exchange<TResult>.Head.Free;
        if(clearItem)
        {
            Item = default!;
        }
        Free = currentFree;
        Exchange<TResult>.Head.Free = this;
    }
}