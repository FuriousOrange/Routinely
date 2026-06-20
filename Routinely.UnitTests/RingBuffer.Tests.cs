namespace Routinely.UnitTests;

[TestClass]
public class RingBufferTests
{
    private RingBuffer<int> ringBuffer;

    [TestInitialize]
    public void TestInitialize()
    {
        ringBuffer = new RingBuffer<int>();
    }

    [TestMethod]
    public void Try_Write_True()
    {
        // Act, Assert
        ringBuffer.TryWrite(1).Should().BeTrue();
    }

    [TestMethod]
    public void Try_Write_False()
    {
        // Arrange
        for (var i = 0; i < ringBuffer.Size - 1; i++)
        {
            ringBuffer.TryWrite(i).Should().BeTrue();
        }

        // Act
        var result = ringBuffer.TryWrite(1);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Try_Read_False()
    {
        // Act, Assert
        var result = 0;
        ringBuffer.TryRead(ref result).Should().BeFalse();
    }

    [TestMethod]
    public void Try_Read_True()
    {
        // Arrange
        ringBuffer.TryWrite(1);

        var result = 0;

        // Act, Assert
        ringBuffer.TryRead(ref result).Should().BeTrue();
        result.Should().Be(1);
    }

    [TestMethod]
    public void TryWrite_Overflow_True()
    {
        // Arrange
        for (var i = 0; i < ringBuffer.Size - 1; i++)
        {
            ringBuffer.TryWrite(i).Should().BeTrue();
        }

        // Act, Assert
        ringBuffer.TryWrite(1).Should().BeFalse();
        ringBuffer.ReadIndex.Should().Be(0);
        ringBuffer.WriteIndex.Should().Be(8191);

        for(var i = 0; i < ringBuffer.Size - 1; i++)
        {
            var result = 0;
            ringBuffer.TryRead(ref result).Should().BeTrue();
            result.Should().Be(i);
        }

        ringBuffer.ReadIndex.Should().Be(8191);

        ringBuffer.TryWrite(1).Should().BeTrue();

        ringBuffer.WriteIndex.Should().Be(0);
    }
}
