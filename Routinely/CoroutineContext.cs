namespace Routinely;

internal sealed class ContextMigrationQueue(CoroutineContext coroutineContext)
{
    private readonly struct MigrationContext(CoroutineStack stack, CoroutineContext targetContext)
    {
        internal readonly CoroutineStack Stack = stack;
        internal readonly CoroutineContext Target = targetContext;
    }

    private readonly CoroutineContext CoroutineContext = coroutineContext;

    private readonly PartitionArray<MigrationContext> migrationContexts = new();

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl]
        private set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Enqueue(CoroutineStack stack, CoroutineContext targetContext)
    {
        while (Count >= migrationContexts.Length)
        {
            migrationContexts.Expand();
        }
        migrationContexts[Count++] = new MigrationContext(stack, targetContext);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Migrate()
    {
        if(Count == 0)
        {
            return 0;
        }

        int migrated = 0;

        for (int i = 0; i < Count; i++)
        {
            var migrationContext = migrationContexts[i];
            var stack = migrationContext.Stack;

            if(stack.DispatcherIndex == -1)
            {
                migrationContexts[i] = default;
                continue;
            }

            CoroutineContext.DetachStack(migrationContext.Stack);
            migrationContext.Target.MigrateStack(migrationContext.Stack);
            migrationContexts[i] = default;
            migrated++;

        }
        Count = 0;

        return migrated;
    }
}

public sealed class CoroutineContext
{
    internal static uint nextId;

    internal ContextMigrationQueue MigrationQueue
    {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl]
        private set;
    }

    internal CoroutineContext(
        PartitionArray<CoroutineStack> stacks,
        int stackCount,
        int currentIndex)
    {
        Stacks = stacks;
        StackCount = stackCount;
        CurrentIndex = currentIndex;
        MigrationQueue = new(this);
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
    internal int MigratePending() => MigrationQueue.Migrate();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnqueueMigration(CoroutineStack stack, CoroutineContext targetContext) 
        => MigrationQueue.Enqueue(stack, targetContext);

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