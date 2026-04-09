namespace Routinely.UnitTests;

[TestClass]
public class CoroutineIdTests
{
    [TestMethod]
    public void GetNextId_Increments_CurrentId()
    {
        // Arrange
        CoroutineId.CurrentId = 0;

        // Act
        var id = CoroutineId.GetNextId();

        // Assert
        id.Should().Be(1);
    }

    [TestMethod]
    public void GetNextId_Wraps_From_Zero_To_One_When_Max()
    {
        // Arrange
        CoroutineId.CurrentId = uint.MaxValue;

        // Act
        var id = CoroutineId.GetNextId();

        // Assert
        id.Should().Be(1);
    }
}
