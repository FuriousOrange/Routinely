using System.Runtime.InteropServices;

namespace Routinely;

[StructLayout(LayoutKind.Explicit, Size = 64)]
internal struct PaddedIndex
{
    [FieldOffset(0)]
    public uint Value;
}

public class RingBuffer<T>
{
    private readonly uint size;
    private readonly uint mask;

    private PaddedIndex readIndex = new();
    private PaddedIndex writeIndex = new();

    private readonly T[] buffer;

    public uint Size => size;

    public uint ReadIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => readIndex.Value & mask;
    }
    public uint WriteIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => writeIndex.Value & mask;
    }

    public bool HasItems
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadIndex != WriteIndex;
    }

    public RingBuffer(uint size = 8192)
    {
        EnsurePowerOfTwo(size);

        this.size = size;
        mask = size - 1;
        buffer = new T[size];
    }

    public bool TryWrite(T item)
    {
        var writeIndex = Volatile.Read(ref this.writeIndex.Value);
        var nextWriteIndex = (writeIndex + 1) & mask;

        if (nextWriteIndex == Volatile.Read(ref readIndex.Value))
        {
            return false; // Buffer is full
        }

        buffer[writeIndex] = item;
        Volatile.Write(ref this.writeIndex.Value, nextWriteIndex);

        return true;
    }

    public bool TryRead(ref T item)
    {
        var readIndex = Volatile.Read(ref this.readIndex.Value);

        if (readIndex == Volatile.Read(ref writeIndex.Value))
        {
            item = default!;
            return false; // Buffer is empty
        }

        item = buffer[readIndex];
        Volatile.Write(ref this.readIndex.Value, (readIndex + 1) & mask);

        return true;
    }

    private static void EnsurePowerOfTwo(uint size)
    {
        if (size == 0 || (size & (size - 1)) != 0)
        {
            throw new ArgumentException("Size must be a power of two.", nameof(size));
        }
    }
}
