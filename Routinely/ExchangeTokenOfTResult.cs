namespace Routinely;

public class ExchangeToken<TResult> : ExchangeToken
{
    internal readonly ExchangeToken<TResult> Head = null!;

    internal ExchangeToken<TResult> Free = null!;
    
    internal TResult Item = default!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExchangeToken(int typeId, ExchangeToken<TResult> head, TResult item) : base(typeId)
    {
        Head = head;
        Item = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Return(bool clearItem = true)
    {
        var currentFree = Head.Free;
        if(clearItem)
        {
            Item = default!;
        }
        Free = currentFree;
        Head.Free = this;
    }
}