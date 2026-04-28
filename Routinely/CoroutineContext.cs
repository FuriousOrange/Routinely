namespace Routinely;

public sealed class CoroutineContext
{
    [ThreadStatic]
    internal static uint nextId;

    internal byte DispatcherId = 0;

    internal CoroutineContext(
        PartitionArray<CoroutineStack> stacks,
        int stackCount,
        int currentIndex)
    {
        Stacks = stacks;
        StackCount = stackCount;
        CurrentIndex = currentIndex;
        DispatcherId = StackDispatcher.Id;
    }

    public uint Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    } = ++nextId;

    public PartitionArray<CoroutineStack> Stacks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    public int StackCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    public int CurrentIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void MigrateStack(CoroutineStack stack)
    {
        while (StackCount >= Stacks.Length)
        {
            Stacks.Expand();
        }


        //// We want to request a new stack here and migrate the old stack over
        //// to the stack in this context. This allows the dispatcher to cleanup the old stack.
        //var newStack = CoroutineStack.Get();
        //newStack.CoroutineContext = this;

        //stack.MigrateStack(newStack);

        stack.CoroutineContext = this;
        stack.DispatcherIndex = StackCount;
        Stacks[StackCount++] = stack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal CoroutineStack GetStack()
    {
        var stack = CoroutineStack.Get();

        if (StackCount >= Stacks.Length)
        {
            Stacks.Expand();
        }

        stack.DispatcherIndex = StackCount;
        stack.CoroutineContext = this;
        Stacks[StackCount++] = stack;

        return stack;
    }

    internal void DetachStack(CoroutineStack stack)
    {
        if(stack.DispatcherIndex == -1)
        {
            return;
        }

        var moveIndex = StackCount - 1;

        Stacks[moveIndex].DispatcherIndex = stack.DispatcherIndex;
        Stacks[stack.DispatcherIndex] = Stacks[moveIndex];
        Stacks[moveIndex] = null!;
        stack.DispatcherIndex = -1;
        stack.Exception = null;
        stack.CoroutineContext = null!;
        StackCount--;
    }
}