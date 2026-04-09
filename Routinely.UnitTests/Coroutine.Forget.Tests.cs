namespace Routinely.UnitTests;

[TestClass]
public class CoroutineForgetTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_Forget_Non_Completed_Sets_Awaited()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        var mainCo = main();

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        mainCo.Forget();
        mainCo.IsAwaited.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Forget_Non_Completed_Sets_Awaited()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        var mainCo = main();

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        mainCo.Forget();
        mainCo.IsAwaited.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Forget_Completed_Does_Not_Throw()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        var act = () => mainCo.Forget();
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Forget_Completed_Does_Not_Throw()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        var act = () => mainCo.Forget();
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Forget_Sync_Completed_Does_Not_Throw()
    {
        // Arrange
        var mainCo = Coroutine.CompletedCoroutine;

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        var act = () => mainCo.Forget();
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Forget_Sync_Completed_Does_Not_Throw()
    {
        // Arrange
        var mainCo = Coroutine.FromResult(1);

        // Act, Assert
        mainCo.IsAwaited.Should().BeFalse();
        var act = () => mainCo.Forget();
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Forget_Faults_And_Does_Not_Use_Exception_Handler()
    {
        // Arrange
        Coroutine.OnUnhandledException = null;

        static async Coroutine main()
        {
            await Coroutine.Yield();
            throw new Exception("Boom!");
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        var act = () => mainCo.ResumeUntil(c => c.HasContext == false);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Forget_Faults_And_Does_Not_Use_Exception_Handler()
    {
        // Arrange
        Coroutine.OnUnhandledException = null;

        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            throw new Exception("Boom!");
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        var act = () => mainCo.ResumeUntil(c => c.HasContext == false);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void Coroutine_Forget_Faults_And_Uses_Exception_Handler()
    {
        // Arrange
        var expectedException = new Exception("Boom!");
        Exception actualException = null;

        Coroutine.ExceptionHandler exceptionHandler = (ex => actualException = ex);
        Coroutine.OnUnhandledException += exceptionHandler;

        async Coroutine main()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        mainCo.ResumeUntil(c => c.HasContext == false);

        // Assert
        actualException.Should().Be(expectedException);

        // Cleanup
        Coroutine.OnUnhandledException -= exceptionHandler;
    }

    [TestMethod]
    public void Coroutine_Of_Result_Forget_Faults_And_Uses_Exception_Handler()
    {
        // Arrange
        var expectedException = new Exception("Boom!");
        Exception actualException = null;

        Coroutine.ExceptionHandler exceptionHandler = (ex => actualException = ex);
        Coroutine.OnUnhandledException += exceptionHandler;

        async Coroutine<int> main()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        mainCo.ResumeUntil(c => c.HasContext == false);

        // Assert
        actualException.Should().Be(expectedException);

        // Cleanup
        Coroutine.OnUnhandledException -= exceptionHandler;
    }
}
