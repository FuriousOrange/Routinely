using System.Diagnostics.Contracts;

namespace Routinely.UnitTests;

[TestClass]
public class RingBufferThreadingTests
{
    private RingBuffer<int> ringBuffer;

    [TestInitialize]
    public void TestInitialize()
    {
        ringBuffer = new RingBuffer<int>();
    }

    [TestMethod]
    public void Thread_Producer()
    {
        // Arrange
        using var barrier = new Barrier(2);

        var done = false;
        var expectedTotal = 0;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            barrier.SignalAndWait();

            for (var i = 0; i < 1_000_000; i++)
            {
                var value = Random.Shared.Next();
                Interlocked.Add(ref expectedTotal, value);
                while (!ringBuffer.TryWrite(value))
                {
                    Thread.SpinWait(1);
                }
            }

            Volatile.Write(ref done, true);
        });

        // Act
        barrier.SignalAndWait();

        var count = 0;
        var result = 0;

        // Continue reading until the writer has finished AND the buffer is empty.
        while (!Volatile.Read(ref done) || ringBuffer.HasItems)
        {
            while (ringBuffer.TryRead(ref result))
            {
                count += result;
            }

            Thread.SpinWait(1);
        }

        // Assert
        count.Should().Be(expectedTotal);
    }
}
