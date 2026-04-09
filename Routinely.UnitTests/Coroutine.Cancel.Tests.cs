namespace Routinely.UnitTests;

[TestClass]
public class CoroutineCancelTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_Cancel()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();
        var mainCo = main();

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancel()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        var mainCo = main();

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Sync()
    {
        // Arrange
        var main = async Coroutine () => { /* sync */};
        var mainCo = main();

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancel_Sync()
    {
        // Arrange
        static async Coroutine<int> main() => 1;
        var mainCo = main();

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Canceling_Non_Awaited_Does_Not_Throw()
    {
        // Arrange
        async Coroutine main() => await Coroutine.Yield();
        var mainCo = main();

        // Act
        mainCo.Cancel();
        var act = () => mainCo.ResumeUntilCompletion();

        // Assert
        act.Should().NotThrow<CancellationException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Canceling_Non_Awaited_Does_Not_Throw()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }
        var mainCo = main();

        // Act
        mainCo.Cancel();
        var act = () => mainCo.ResumeUntilCompletion();

        // Assert
        act.Should().NotThrow<CancellationException>();
    }

    [TestMethod]
    public void Coroutine_Cancel_Also_Cancels_Awaited()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();
        var subCo = sub();

        var main = async Coroutine () => await subCo;
        var mainCo = main();

        // Act 
        mainCo.Cancel();

        // Act
        mainCo.IsCanceled.Should().BeTrue();
        subCo.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public async Task Coroutine_Of_Result_Cancel_Also_Cancels_Awaited()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }
        var subCo = sub();

        var main = async Coroutine () => await subCo;
        var mainCo = main();

        // Act 
        mainCo.Cancel();

        // Act
        mainCo.IsCanceled.Should().BeTrue();
        subCo.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Also_Cancels_Nested_Awaiteds()
    {
        // Arrange
        static async Coroutine sub2()
        {
            await Coroutine.Yield();
        }

        var sub2Co = sub2();

        async Coroutine sub1()
        {
            await sub2Co;
        }
        var sub1Co = sub1();

        async Coroutine main() => await sub1Co;

        var mainCo = main();

        // Act 
        mainCo.Cancel();

        // Act
        mainCo.IsCanceled.Should().BeTrue();
        sub1Co.IsCanceled.Should().BeTrue();
        sub2Co.IsCanceled.Should().BeTrue();
    }


    [TestMethod]
    public void Coroutine_Of_Result_Cancel_Also_Cancels_Nested_Awaiteds()
    {
        // Arrange
        static async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            return 1;
        }

        var sub2Co = sub2();

        async Coroutine<int> sub1() => await sub2Co;

        var sub1Co = sub1();

        async Coroutine main() => await sub1Co;

        var mainCo = main();

        // Act 
        mainCo.Cancel();

        // Act
        mainCo.IsCanceled.Should().BeTrue();
        sub1Co.IsCanceled.Should().BeTrue();
        sub2Co.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Awaited_Throws_Invalid_Cancellation()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        var subCo = sub();

        async Coroutine main() => await subCo;
        var mainCo = main();

        // Act
        var act = () => subCo.Cancel();

        // Asert
        act.Should().Throw<AwaitedCoroutineException>()
            .WithMessage(AwaitedCoroutineException.InvalidCancellationMessage);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancel_Awaited_Throws_Invalid_Cancellation()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var subCo = sub();

        async Coroutine<int> main() => await subCo;
        var mainCo = main();

        // Act
        var act = () => subCo.Cancel();

        // Asert
        act.Should().Throw<AwaitedCoroutineException>()
            .WithMessage(AwaitedCoroutineException.InvalidCancellationMessage);
    }

    [TestMethod]
    public void Coroutine_Cancel_No_Context_Does_Not_Throw()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        var mainCo = main();
        mainCo.ResumeUntil(c => !c.HasContext);

        // Act
        var act = () => mainCo.Cancel();

        // Assert
        act.Should().NotThrow<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancel_No_Context_Does_Not_Throw()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        var mainCo = main();
        mainCo.ResumeUntil(c => !c.HasContext);

        // Act
        var act = () => mainCo.Cancel();

        // Assert
        act.Should().NotThrow<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_On_Yield()
    {
        // Arrange
        var cleanedUp = false;

        async Coroutine main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
            }
            finally
            {
                cleanedUp = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanedUp.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancellation_Contract_On_Yield()
    {
        // Arrange
        var cleanedUp = false;

        async Coroutine<int> main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                return 1;
            }
            finally
            {
                cleanedUp = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanedUp.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Cancels_Awaited_With_Contract()
    {
        // Arrange
        var subCleanup = false;
        var mainCleanup = false;

        async Coroutine sub()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);
            }
            finally
            {
                subCleanup = true;
            }
        }

        var subCo = sub();

        async Coroutine main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                await subCo.Enforce(contract);
                await subCo.Enforce(contract);
            }
            finally
            {
                mainCleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        mainCleanup.Should().BeTrue();

        subCo.IsCompleted.Should().BeTrue();
        subCo.IsCanceled.Should().BeTrue();
        subCo.IsFaulted.Should().BeFalse();
        subCleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancellation_Contract_Cancels_Awaited_With_Contract()
    {
        // Arrange
        var subCleanup = false;
        var mainCleanup = false;

        async Coroutine<int> sub()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);
                return 1;
            }
            finally
            {
                subCleanup = true;
            }
        }

        var subCo = sub();

        async Coroutine<int> main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                var r = await subCo.Enforce(contract);
                return await subCo.Enforce(contract);
            }
            finally
            {
                mainCleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        mainCleanup.Should().BeTrue();

        subCo.IsCompleted.Should().BeTrue();
        subCo.IsCanceled.Should().BeTrue();
        subCo.IsFaulted.Should().BeFalse();
        subCleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Non_Awaiting_Enforce_Does_Not_Propagate_Exception()
    {
        // Arrange
        var cleanup = false;

        async Coroutine main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    await Coroutine.Yield().Enforce(contract);

                    contract.Enforce();
                }
            }
            finally
            {
                cleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancellation_Contract_Non_Awaiting_Enforce_Does_Not_Propagate_Exception()
    {
        // Arrange
        var cleanup = false;

        async Coroutine<int> main()
        {
            using var contract = await CancellationContract.Enter();

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    await Coroutine.Yield().Enforce(contract);

                    contract.Enforce();
                }

                return 1;
            }
            finally
            {
                cleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Enter_On_Already_Awaiting_Coroutine()
    {
        // Arrange
        var cleanup = false;

        async Coroutine main()
        {
            // No contract here, but we will attempt to enter one while already awaiting on resume
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);
            }
            finally
            {
                cleanup = true;
            }
        }

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancellation_Contract_Enter_On_Already_Awaiting_Coroutine()
    {
        // Arrange
        var cleanup = false;

        async Coroutine<int> main()
        {
            // No contract here, but we will attempt to enter one while already awaiting on resume
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);

                return 1;
            }
            finally
            {
                cleanup = true;
            }
        }

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        mainCo.IsFaulted.Should().BeFalse();
        cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Nested_Contracts_Chain_Completes_NoCancel()
    {
        // Arrange
        var mainCleanup = false;
        var sub1Cleanup = false;
        var sub2Cleanup = false;

        async Coroutine sub2()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);

            }
            finally
            {
                sub2Cleanup = true;
            }
        }

        async Coroutine sub1()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                await sub2().Enforce(contract);

            }
            finally
            {
                sub1Cleanup = true;
            }
        }

        async Coroutine main()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await sub1().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);
            }
            finally
            {
                mainCleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCleanup.Should().BeTrue();
        sub1Cleanup.Should().BeTrue();
        sub2Cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cancellation_Contract_Nested_Contracts_Chain_Completes_NoCancel()
    {
        // Arrange
        var mainCleanup = false;
        var sub1Cleanup = false;
        var sub2Cleanup = false;

        async Coroutine<int> sub2()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);

                return 1;
            }
            finally
            {
                sub2Cleanup = true;
            }
        }

        async Coroutine<int> sub1()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(contract);
                return await sub2().Enforce(contract);

            }
            finally
            {
                sub1Cleanup = true;
            }
        }

        async Coroutine<int> main()
        {
            await Coroutine.Yield();

            using var contract = await CancellationContract.Enter();

            try
            {
                return await sub1().Enforce(contract);
                await Coroutine.Yield().Enforce(contract);
            }
            finally
            {
                mainCleanup = true;
            }
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCleanup.Should().BeTrue();
        sub1Cleanup.Should().BeTrue();
        sub2Cleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Await_In_Finally()
    {

        // Arrange
        var finished = false;

        async Coroutine sub()
        {
            await Coroutine.Yield();
            finished = true;
        }

        async Coroutine main()
        {
            using var ctc = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
            }
            catch (CancellationException)
            {
                await sub().Enforce(ctc);
            }
        }

        var mainCo = main();
        mainCo.Cancel();
        StackDispatcher.MoveAllNext();

        DrainCoroutines();

        finished.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Can_Start_Async_Work_When_CancellationException_Handled()
    {
        // Arrange
        var finished = false;

        async Coroutine sub2()
        {
            await Coroutine.Yield();
            finished = true;
        }

        async Coroutine sub1()
        {
            using var ctc = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
            }
            catch (CancellationException)
            {
                await sub2().Enforce(ctc);
            }
        }

        async Coroutine main()
        {
            using var ctc = await CancellationContract.Enter();

            await sub1().Enforce(ctc);
        }

        // Act
        var mainCo = main();
        mainCo.Cancel();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();
        DrainCoroutines();
        finished.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancel_Cleanup_Work_Starts_On_New_Stack()
    {
        // Arrange
        var subStarted = false;
        var subFinished = false;

        async Coroutine sub()
        {
            subStarted = true;
            await Coroutine.Yield();
            await Coroutine.Yield();
            subFinished = true;
        }

        async Coroutine main()
        {
            using var ctc = await CancellationContract.Enter();

            try
            {
                await Coroutine.Yield().Enforce(ctc);
                await Coroutine.Yield().Enforce(ctc);
            }
            catch (CancellationException)
            {
                await sub().Enforce(ctc);  // Still holds ctc reference
            }
            // ctc.Dispose() happens here
        }

        // Act, Assert
        var mainCo = main();
        StackDispatcher.StackCount.Should().Be(1); // main is on the stack
        mainCo.Cancel();

        // Critical assertion: Proves stack resurrection pattern
        // When catch block starts sub(), main's stack is ready for cleanup (Completed|Canceled),
        // so sub() gets its own stack instead of sharing
        StackDispatcher.StackCount.Should().Be(2); // main + sub on separate stacks

        StackDispatcher.MoveAllNext();  // main completes, disposes ctc

        StackDispatcher.StackCount.Should().Be(1); // sub is on the stack

        subStarted.Should().BeTrue();
        subFinished.Should().BeFalse(); // Still running after parent disposed

        DrainCoroutines();              // Finish sub()

        subFinished.Should().BeTrue();
    }
}

