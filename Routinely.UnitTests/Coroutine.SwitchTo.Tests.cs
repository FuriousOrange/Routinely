namespace Routinely.UnitTests;

[TestClass]
public class CoroutineSwitchToTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_Can_Switch_To_Sync_Coroutine()
    {
        // Arrange
        var count = 0;
        async Coroutine next()
        {
            count++;
        }

        async Coroutine main() => await Coroutine.SwitchTo(next);

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine()
    {
        // Arrange
        var count = 0;
        async Coroutine next()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main() => await Coroutine.SwitchTo(next);

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo()
    {
        // Arrange
        var count = 0;
        async Coroutine next()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next);
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine_That_Yields_To_Another_Sync()
    {
        // Arrange
        var count = 0;
        async Coroutine next2()
        {
            count++;
        }

        async Coroutine next1() => await Coroutine.SwitchTo(next2);

        async Coroutine main() => await Coroutine.SwitchTo(next1);

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine_That_Yields_To_Another_Async()
    {
        // Arrange
        var count = 0;
        async Coroutine next2()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine next1() => await Coroutine.SwitchTo(next2);

        async Coroutine main() => await Coroutine.SwitchTo(next1);

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        // Critical assertion: Ensures the stack is being cleared of any previous coroutines that have been yielded to
        // before resuming the yield to, preventing infinite stack growth.
        mainCo.Stack.HeadIndex.Should().Be(2);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo_Another_Async_That_Yields_Then_Yields_To_Sync()
    {
        // Arrange
        var count = 0;
        async Coroutine next2()
        {
            count++;
        }

        async Coroutine next1()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next2);
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next1);
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo_Another_Async_That_Yields_Then_Yields_To()
    {
        // Arrange
        var count = 0;
        async Coroutine next2()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine next1()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next2);
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next1);
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Sync(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine next(int maxDepth)
        {
            if (maxDepth == 0)
                return;

            count++;

            await Coroutine.SwitchTo(() => next(maxDepth - 1));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Async(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine next(int maxDepth)
        {
            if (maxDepth == 0)
                return;

            count++;

            await Coroutine.Yield();
            await Coroutine.SwitchTo(() => next(maxDepth - 1));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    public void Coroutine_Nested_With_Switch_To_Async_Stack_Cleanup()
    {
        // Arrange
        var startStacks = Coroutine.Count;

        async Coroutine next()
        {
            await Coroutine.Yield();
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(next);
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        mainCo.ResumeUntil(c => !c.HasContext);

        // Assert
        Coroutine.Count.Should().Be(startStacks);
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Async_Call_Chain_Mid_Stack(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine sub(int maxDepth)
        {
            if (maxDepth == 0)
            {
                await Coroutine.Yield();
                return;
            }

            count++;

            await sub(maxDepth - 1);
        }

        async Coroutine next(int maxDepth)
        {
            count++;

            await Coroutine.SwitchTo(() => sub(maxDepth - 1));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    public void Coroutine_Cancel_Cancels_YieldedTo()
    {
        // Arrange
        static async Coroutine next() => await Coroutine.Yield();

        static async Coroutine main() => await Coroutine.SwitchTo(next);

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        mainCo.Cancel();
        mainCo.Stack.HeadIndex.Should().Be(2);
        mainCo.Stack.Tokens[0].Item.HasFlag(CoroutineCore.Canceled).Should().BeTrue();
        mainCo.Stack.Tokens[1].Item.HasFlag(CoroutineCore.Canceled).Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Clean_Up_On_Switch_To()
    {
        // Arrange
        var mainCleanup = false;

        async Coroutine next() => await Coroutine.Yield();

        async Coroutine main()
        {
            using var ctc = await CancellationContract.Enter();

            try
            {
                await Coroutine.SwitchTo(next).Enforce(ctc);
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
        mainCleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Switch_To_Next_Throws_Faults_Sync()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine next() => throw expectedException;

        async Coroutine main() => await Coroutine.SwitchTo(next);

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsFaulted.Should().BeTrue();
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Switch_To_Next_Throws_Faults_Async()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine next()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine main() => await Coroutine.SwitchTo(next);

        // Act
        var mainCo = main();
        var act = () => mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        act.Should().Throw<Exception>().And.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Infinite_Switch_To_Can_Be_Cancelled()
    {
        // Arrange
        var loopBreaker = false;
        static async Coroutine toMe() => await Coroutine.SwitchTo(toYou);

        static async Coroutine toYou() => await Coroutine.SwitchTo(toMe);

        var mainCo = toMe();

        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();
        }

        // Act
        mainCo.Cancel();
        mainCo.IsCompleted.Should().BeTrue();

        // Assert
        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();

            if (Coroutine.Count == 0)
            {
                loopBreaker = true;
                break;
            }
        }

        loopBreaker.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Sync_Coroutine_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next(int ctx)
        {
            count++;
        }

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next(ctx));

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next(int ctx)
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next(ctx));

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next(int ctx)
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(0, ctx => next(ctx));
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine_That_Yields_To_Another_Sync_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next2(int ctx)
        {
            count++;
        }

        async Coroutine next1(int ctx) => await Coroutine.SwitchTo(ctx, c => next2(c));

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next1(ctx));

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Switch_To_Async_Coroutine_That_Yields_To_Another_Async_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next2(int ctx)
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine next1(int ctx) => await Coroutine.SwitchTo(ctx, c => next2(c));

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next1(ctx));

        // Act
        var mainCo = main();

        // Assert
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        Coroutine.ResumeAll();
        count.Should().Be(0);
        // Critical assertion: Ensures the stack is being cleared of any previous coroutines that have been yielded to
        // before resuming the yield to, preventing infinite stack growth.
        mainCo.Stack.HeadIndex.Should().Be(2);
        Coroutine.ResumeAll();
        count.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo_Another_Async_That_Yields_Then_Yields_To_Sync_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next2(int ctx)
        {
            count++;
        }

        async Coroutine next1(int ctx)
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(ctx, c => next2(c));
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(0, ctx => next1(ctx));
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_Yields_Then_YieldsTo_Another_Async_That_Yields_Then_Yields_To_With_Context()
    {
        // Arrange
        var count = 0;
        async Coroutine next2(int ctx)
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine next1(int ctx)
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(ctx, c => next2(c));
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(0, ctx => next1(ctx));
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => count == 1);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Sync_With_Context(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine next(int maxDepth)
        {
            if (maxDepth == 0)
                return;

            count++;

            await Coroutine.SwitchTo(maxDepth - 1, d => next(d));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Async_With_Context(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine next(int maxDepth)
        {
            if (maxDepth == 0)
                return;

            count++;

            await Coroutine.Yield();
            await Coroutine.SwitchTo(maxDepth - 1, d => next(d));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    public void Coroutine_Nested_With_Switch_To_Async_Stack_Cleanup_With_Context()
    {
        // Arrange
        var startStacks = Coroutine.Count;

        async Coroutine next(int ctx)
        {
            await Coroutine.Yield();
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.SwitchTo(0, ctx => next(ctx));
        }

        // Act
        var mainCo = main();
        mainCo.Forget();
        mainCo.ResumeUntil(c => !c.HasContext);

        // Assert
        Coroutine.Count.Should().Be(startStacks);
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(15)]
    public void Coroutine_Nested_With_Switch_To_Async_Call_Chain_Mid_Stack_With_Context(int depth)
    {
        // Arrange
        var count = 0;

        async Coroutine sub(int maxDepth)
        {
            if (maxDepth == 0)
            {
                await Coroutine.Yield();
                return;
            }

            count++;

            await sub(maxDepth - 1);
        }

        async Coroutine next(int maxDepth)
        {
            count++;

            await Coroutine.SwitchTo(maxDepth - 1, d => sub(d));
        }

        // Act
        var nextCo = next(depth);
        nextCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(depth);
    }

    [TestMethod]
    public void Coroutine_Cancel_Cancels_YieldedTo_With_Context()
    {
        // Arrange
        static async Coroutine next(int ctx) => await Coroutine.Yield();

        static async Coroutine main() => await Coroutine.SwitchTo(0, static ctx => next(ctx));

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        mainCo.Cancel();
        mainCo.Stack.HeadIndex.Should().Be(2);
        mainCo.Stack.Tokens[0].Item.HasFlag(CoroutineCore.Canceled).Should().BeTrue();
        mainCo.Stack.Tokens[1].Item.HasFlag(CoroutineCore.Canceled).Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Cancellation_Contract_Clean_Up_On_Switch_To_With_Context()
    {
        // Arrange
        var mainCleanup = false;

        static async Coroutine next(int ctx) => await Coroutine.Yield();

        async Coroutine main()
        {
            using var ctc = await CancellationContract.Enter();

            try
            {
                await Coroutine.SwitchTo(0, static ctx => next(ctx)).Enforce(ctc);
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
        mainCleanup.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Switch_To_Next_Throws_Faults_Sync_With_Context()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine next(int ctx) => throw expectedException;

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next(ctx));

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsFaulted.Should().BeTrue();
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Switch_To_Next_Throws_Faults_Async_With_Context()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine next(int ctx)
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine main() => await Coroutine.SwitchTo(0, ctx => next(ctx));

        // Act
        var mainCo = main();
        var act = () => mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        act.Should().Throw<Exception>().And.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Infinite_Switch_To_Can_Be_Cancelled_With_Context()
    {
        // Arrange
        var loopBreaker = false;
        static async Coroutine toMe(int ctx) => await Coroutine.SwitchTo(ctx, static c => toYou(c));

        static async Coroutine toYou(int ctx) => await Coroutine.SwitchTo(ctx, static c => toMe(c));

        var mainCo = toMe(0);

        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();
        }

        // Act
        mainCo.Cancel();
        mainCo.IsCompleted.Should().BeTrue();

        // Assert
        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();

            if (Coroutine.Count == 0)
            {
                loopBreaker = true;
                break;
            }
        }

        loopBreaker.Should().BeTrue();
    }

}
