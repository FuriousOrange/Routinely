namespace Routinely.UnitTests;

[TestClass]
public class CoroutineOperatorsTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_Equality_Equals()
    {
        // Arrange
        Coroutine a = Coroutine.CompletedCoroutine;
        Coroutine b = a;

        // Act
        var result = a == b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Equality_Equals()
    {
        // Arrange
        Coroutine<int> a = Coroutine.FromResult(1);
        Coroutine<int> b = a;

        // Act
        var result = a == b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_To_Coroutine_Equality_Equals()
    {
        // Arrange
        Coroutine<int> a = Coroutine.FromResult(1);
        Coroutine b = a;

        // Act
        var result = a == b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Equality_Not_Equals()
    {
        // Arrange
        Coroutine a = Coroutine.CompletedCoroutine;
        Coroutine b = Coroutine.CompletedCoroutine;

        // Act
        var result = a != b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Equality_Not_Equals()
    {
        // Arrange
        Coroutine<int> a = Coroutine.FromResult(1);
        Coroutine<int> b = Coroutine.FromResult(1);

        // Act
        var result = a != b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_To_Coroutine_Of_Result_Equality_Not_Equals()
    {
        // Arrange
        Coroutine a = Coroutine.CompletedCoroutine;
        Coroutine<int> b = Coroutine.FromResult(1);

        // Act
        var result = a != b;

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_To_Coroutine_Equality_Not_Equals()
    {
        // Arrange
        Coroutine<int> a = Coroutine.FromResult(1);
        Coroutine b = Coroutine.CompletedCoroutine;

        // Act
        var result = a != b;

        // Assert
        result.Should().BeTrue();
    }
}
