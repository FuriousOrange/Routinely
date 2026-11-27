namespace Routinely;

public sealed class CoroutineContext
{
    [ThreadStatic]
    internal static uint nextId;

    internal byte DispatcherId = 0;

    internal CoroutineContext(
        PartitionArray<CoroutineStack> stacks,
        int stackCount,
        int currentIndex,
        double totalElapsedSeconds)
    {
        Stacks = stacks;
        StackCount = stackCount;
        CurrentIndex = currentIndex;
        DispatcherId = StackDispatcher.Id;
        TotalElapsedSeconds = totalElapsedSeconds;
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

    public double DeltaSeconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    public TimeSpan DeltaTimeSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TimeSpan.FromSeconds(DeltaSeconds);
    }

    public double TotalElapsedSeconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    public TimeSpan TotalElapsedTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TimeSpan.FromSeconds(TotalElapsedSeconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Cancel()
    {
        var cancelLimit = StackCount;

        for (var i = 0; i < cancelLimit; i++)
        {
            Stacks[i].Cancel();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void MigrateStack(CoroutineStack stack)
    {
        while (StackCount >= Stacks.Length)
        {
            Stacks.Expand();
        }

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