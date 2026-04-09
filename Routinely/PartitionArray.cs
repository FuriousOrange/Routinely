namespace Routinely;

public class PartitionArray<T>
{
    private const int PartitionShift = 13;                  // log2(MaxPartitionSize)
    private const int PartitionMask = MaxPartitionSize - 1;

    internal const int MaxPartitionSize = 8192;

    private int length = 0;
    private T[][] partitions;

    public int Length => length;

    public int PartitionCount => partitions.Length;

    public PartitionArray()
    {
        partitions = new T[1][];
        partitions[0] = new T[MaxPartitionSize];

        SetLength();
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var partition = index >> PartitionShift;
            var innerIndex = index & PartitionMask;
            return partitions[partition][innerIndex];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var partition = index >> PartitionShift;
            var innerIndex = index & PartitionMask;
            partitions[partition][innerIndex] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Expand()
    {
        var partition = length >> PartitionShift;
        var innerIndex = length & PartitionMask;

        if (partition >= partitions.Length)
        {
            Array.Resize(ref partitions, partitions.Length + 1);
        }
        if (partitions[partition] == null)
        {
            partitions[partition] = new T[MaxPartitionSize];
        }
        if (innerIndex >= partitions[partition].Length)
        {
            Array.Resize(ref partitions[partition], partitions[partition].Length * 2);
        }

        SetLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetLength() => length = ((partitions.Length - 1) * MaxPartitionSize) + partitions[^1].Length;
}