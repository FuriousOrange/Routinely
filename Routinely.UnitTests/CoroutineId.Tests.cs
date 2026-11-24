namespace Routinely.UnitTests;

[TestClass]
public class CoroutineIdTests
{
    [TestMethod]
    public void GetNextId_Increments_CurrentId()
    {
        // Arrange
        IdFactory<object>.CurrentId = 0;

        // Act
        var id = IdFactory<object>.GetNext();

        // Assert
        id.Should().Be(1);
    }

    [TestMethod]
    public void GetNextId_Wraps_From_Zero_To_One_When_Max()
    {
        // Arrange
        IdFactory<object>.CurrentId = uint.MaxValue;

        // Act
        var id = IdFactory<object>.GetNext();

        // Assert
        id.Should().Be(1);
    }
}
