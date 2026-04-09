namespace Routinely.UnitTests;

[TestClass]
public class PartitionArrayTests
{
    [TestMethod]
    public void Expand_Adds_New_Partitions()
    {
        // Arrange
        var partitionArray = new PartitionArray<int>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            // We already have an initial partition, so we need to expand 3 times to get to 4 partitions
            partitionArray.Expand();
        }

        // Assert
        partitionArray.PartitionCount.Should().Be(4);
        partitionArray.Length.Should().Be(PartitionArray<int>.MaxPartitionSize * 4);

    }
}
