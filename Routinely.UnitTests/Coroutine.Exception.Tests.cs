namespace Routinely.UnitTests;

[TestClass]
public class CoroutineExceptionTests : CoroutineTestBase
{
    [TestMethod]
    public void Faulted_Coroutine_Throws_After_Dispatcher_Has_Resumed_All_Single_Fault()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine fault()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine main() => await Coroutine.Yield();

        // Act
        var act = () =>
        {
            _ = fault();
            _ = main();

            while (Coroutine.ResumeAll()) ;
        };

        // Assert
        act.Should().Throw<Exception>().And.Should().Be(expectedException);
        Coroutine.Count.Should().Be(0);
    }

    [TestMethod]
    public void Faulted_Coroutine_Throws_After_Dispatcher_Has_Resumed_All_Multiple_Faults()
    {
        // Arrange
        var expectedException1 = new Exception("Boom!");

        async Coroutine fault1()
        {
            await Coroutine.Yield();
            throw expectedException1;
        }

        var expectedException2 = new Exception("Boom!");

        async Coroutine fault2()
        {
            await Coroutine.Yield();
            throw expectedException2;
        }

        // Act
        var act = () =>
        {
            _ = fault1();
            _ = fault2();

            while (Coroutine.ResumeAll()) ;
        };

        // Assert
        act.Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().ContainInOrder(expectedException1, expectedException2);
            
        Coroutine.Count.Should().Be(0);
    }

    [TestMethod]
    public void Faulted_Coroutine_Uses_ExceptionHandler()
    {
        // Arrange
        var expectedException = new Exception("Boom!");
        Exception handledException = null;

        void ExceptionHandler(Exception ex)
        {
            handledException = ex;
        }

        Coroutine.OnUnhandledException += ExceptionHandler;

        async Coroutine fault()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine main() => await Coroutine.Yield();

        // Act
        var act = () =>
        {
            _ = fault();
            _ = main();

            while (Coroutine.ResumeAll()) ;
        };

        // Assert
        act.Should().NotThrow();
        Coroutine.Count.Should().Be(0);
        handledException.Should().Be(expectedException);

        // Cleanup
        Coroutine.OnUnhandledException -= ExceptionHandler;
    }
}

