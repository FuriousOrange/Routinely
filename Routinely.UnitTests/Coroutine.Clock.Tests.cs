namespace Routinely.UnitTests;

[TestClass]
public class CoroutineClockTests : CoroutineTestBase
{
    [TestMethod]
    public void Update_Increases_Delta()
    {
        // Act
        Coroutine.Clock.Update(1);

        // Assert
        Coroutine.DeltaSeconds.Should().Be(1);
        Coroutine.TotalElapsedSeconds.Should().Be(1);
    }

    [TestMethod]
    public void Update_Increases_Delta_For_All_Contexts()
    {
        // Arrange
        var c1 = Coroutine.CreateContext();
        var c2 = Coroutine.CreateContext();

        // Act
        Coroutine.Clock.Update(1);

        // Assert
        c1.DeltaSeconds.Should().Be(1);
        c2.DeltaSeconds.Should().Be(1);
        Coroutine.MainContext.DeltaSeconds.Should().Be(1);

        c1.TotalElapsedSeconds.Should().Be(1);
        c2.TotalElapsedSeconds.Should().Be(1);
        Coroutine.MainContext.TotalElapsedSeconds.Should().Be(1);
    }

    [TestMethod]
    public void Update_Clears_Delta_After_ResumeAll()
    {
        // Act
        Coroutine.Clock.Update(1);

        // Act
        Coroutine.ResumeAll();

        // Assert
        Coroutine.DeltaSeconds.Should().Be(0);
        Coroutine.TotalElapsedSeconds.Should().Be(1);
    }

    [TestMethod]
    public void Update_Does_Not_Clear_Delta_On_Inactive_ContextsAfter_ResumeAll()
    {
        // Act
        var c1 = Coroutine.CreateContext();
        var c2 = Coroutine.CreateContext();

        Coroutine.Clock.Update(1);

        // Act
        Coroutine.ResumeAll();

        // Assert
        Coroutine.DeltaSeconds.Should().Be(0);
        Coroutine.TotalElapsedSeconds.Should().Be(1);

        c1.DeltaSeconds.Should().Be(1);
        c1.TotalElapsedSeconds.Should().Be(1);

        c2.DeltaSeconds.Should().Be(1);
        c2.TotalElapsedSeconds.Should().Be(1);
    }

    [TestMethod]
    public void Update_Advances_Clock_Delay_Coroutine()
    {
        // Arrange
        var mainCo = Coroutine.Clock.Delay(1);

        // Act, Assert
        Coroutine.Clock.Update(0.1);
        mainCo.IsCompleted.Should().BeFalse();

        Coroutine.Clock.Update(0.9);
        Coroutine.ResumeAll();

        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(double.NaN)]
    public void Update_Invalid_Seconds_Throws(double seconds)
    {
        // Act
        var act = () => Coroutine.Clock.Update(seconds);

        // Assert
        act.Should().Throw<CoroutineClockException>()
            .WithMessage(CoroutineClockException.ClockUpdatePositiveFiniteMessage);
    }

    [TestMethod]
    public void Update_Delta_Cleared_After_Fault()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine main()
        {
            await Coroutine.Yield();
            throw expectedException;
            
        }

        var mainCo = main();

        Coroutine.Clock.Update(1);

        // Act
        var act = () => mainCo.ResumeUntilCompletion();

        Coroutine.DeltaSeconds.Should().Be(1);
        act.Should().Throw<Exception>()
            .And.Should().Be(expectedException);
        Coroutine.DeltaSeconds.Should().Be(0);
    }

    [TestMethod]
    public void Resume_Delay_Coroutine_Without_Clock_Update_Throws()
    {
        // Act
        var mainCo = Coroutine.Clock.Delay(1);

        // Act
        var act = () => mainCo.ResumeUntilCompletion();

        // Assert
        act.Should().Throw<CoroutineClockException>()
            .WithMessage(CoroutineClockException.ClockNotUpdatedMessage);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(double.NaN)]
    public void Delay_Invalid_Seconds_Throws(double seconds)
    {
        // Act
        var act = () => Coroutine.Clock.Delay(seconds);

        // Assert
        act.Should().Throw<CoroutineClockException>()
            .WithMessage(CoroutineClockException.DelayUpdateZeroOrPositiveFiniteMessage);
    }

    [TestMethod]
    public void Delay_Zero_Completes_Sync()
    {
        // Arrange
        var mainCo = Coroutine.Clock.Delay(0);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Delay_Accumulated_Inactive_Time_Accounted_For()
    {
        // Arrange
        var c1 = Coroutine.CreateContext();
        var mainCo = Coroutine.Clock.Delay(1);
        Coroutine.SetContext(c1);

        // Act, Assert
        Coroutine.Clock.Update(1);
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll(); // Clears the c1 context's delta, not the main context's.

        Coroutine.Clock.Update(0.1);
        Coroutine.SetContext(Coroutine.MainContext);
        Coroutine.ResumeAll();

        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Delay_Does_Not_Count_Time_Accumulated_Before_Start()
    {
        // Arrange
        var c1 = Coroutine.CreateContext();

        // Act, Assert
        Coroutine.Clock.Update(1);
        Coroutine.ResumeAll(); // Clears the main context's delta, not the inactive context's.
        Coroutine.SetContext(c1);

        var delay = Coroutine.Clock.Delay(1);

        Coroutine.Clock.Update(0.1);
        Coroutine.ResumeAll();

        delay.IsCompleted.Should().BeFalse();

        Coroutine.Clock.Update(0.9);
        Coroutine.ResumeAll();

        delay.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Delay_Migrated_To_Later_Context_Preserves_Elapsed_Time()
    {
        // Arrange
        Coroutine.Clock.Update(10);
        var mainCo = Coroutine.Clock.Delay(2);

        Coroutine.Clock.Update(0.5);
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeFalse();

        var destination = Coroutine.CreateContext();

        // Act, Assert

        // Schedule and perform migration from the main context.
        mainCo.SetContext(destination);
        Coroutine.ResumeAll();

        Coroutine.MainContext.StackCount.Should().Be(0);
        destination.StackCount.Should().Be(1);

        // One second has elapsed since the delay started.
        Coroutine.Clock.Update(0.5);
        Coroutine.SetContext(destination);
        Coroutine.ResumeAll();

        mainCo.IsCompleted.Should().BeFalse();

        // The full two seconds have now elapsed.
        Coroutine.Clock.Update(1);
        Coroutine.ResumeAll();

        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Delay_After_Async_Context_Migration_Uses_Destination_Clock()
    {
        // Arrange
        Coroutine.Clock.Update(10);
        var destination = Coroutine.CreateContext();
        var enteredDestination = false;

        async Coroutine main()
        {
            await Coroutine.Context(destination);
            enteredDestination = true;
            await Coroutine.Clock.Delay(2);
        }

        var mainCo = main();

        // Act, Assert

        // Move the coroutine from the main context to the destination.
        Coroutine.ResumeAll();
        Coroutine.MainContext.StackCount.Should().Be(0);
        destination.StackCount.Should().Be(1);

        // Resume in the destination and start the delay.
        Coroutine.SetContext(destination);
        Coroutine.ResumeAll();

        enteredDestination.Should().BeTrue();
        mainCo.IsCompleted.Should().BeFalse();

        Coroutine.Clock.Update(0.5);
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeFalse();

        Coroutine.Clock.Update(1.5);
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeTrue();
    }
}
