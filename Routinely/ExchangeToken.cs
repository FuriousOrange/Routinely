namespace Routinely;

public class ExchangeToken
{
    internal static int TypeIdCounter = 0;

    internal readonly int TypeId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExchangeToken(int typeId)
    {
        TypeId = typeId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void CheckedGet<T>(ref T item)
    {
        if (this is ExchangeToken<T> typedToken)
        {
            item = typedToken.Item;
        }
    }
}
