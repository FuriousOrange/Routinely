namespace Routinely.UnitTests;

[TestClass]
public class CoroutineAwaitTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_Has_Context()
    {
        // Arrange
        static async Coroutine main() { }

        // Act
        var mainCo = main();

        // Assert
        mainCo.HasContext.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Has_Context()
    {
        // Arrange
        static async Coroutine<int> main() => 1;

        // Act
        var mainCo = main();

        // Assert
        mainCo.HasContext.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Has_No_Context()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();
        Coroutine.ResumeAll();

        // Assert
        mainCo.HasContext.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Has_No_Context()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        // Act
        var mainCo = main();
        Coroutine.ResumeAll();
        Coroutine.ResumeAll();

        // Assert
        mainCo.HasContext.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Completes_Synchronously()
    {
        // Arrange
        static async Coroutine main() { /* sync */ }

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Completes_Synchronously()
    {
        // Arrange
        static async Coroutine<int> main() => 1;

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Completes_Nested_Synchronously()
    {
        // Arrange
        static async Coroutine sub() { /* sync */ }

        static async Coroutine main() => await sub();

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_of_Result_Completes_Nested_Synchronously()
    {
        // Arrange
        static async Coroutine<int> sub() => 1;

        static async Coroutine<int> main() => await sub();

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Yield()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Yield()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public void Coroutine_Can_Complete_After_Yield()
    {
        // Arrange
        static async Coroutine main() => await Coroutine.Yield();

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Complete_After_Yield()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Complete_After_Mulitple_Yields()
    {
        // Arrange
        static async Coroutine main()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Complete_After_Mulitple_Yields()
    {
        // Arrange
        static async Coroutine<int> main()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 1;
        }

        // Act
        var mainCo = main();

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeFalse();
        Coroutine.ResumeAll();
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Await_A_Sub_Coroutine()
    {
        // Arrange
        var count = 0;

        async Coroutine sub()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main() => await sub();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert;
        mainCo.IsCompleted.Should().BeTrue();
        count.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Await_A_Sub_Coroutine()
    {
        // Arrange
        async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        async Coroutine<int> main() => await sub();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert;
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Await_Two_Sub_Coroutines()
    {
        // Arrange
        var count = 0;

        async Coroutine sub()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine main()
        {
            await sub();
            await sub();
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(2);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Await_Two_Sub_Coroutines()
    {
        // Arrange
        async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        async Coroutine<int> main()
        {
            var result = 0;
            result += await sub();
            result += await sub();

            return result;
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(2);
    }

    [TestMethod]
    public void Coroutine_Can_Await_Nested_Coroutine()
    {
        // Arrange
        var count = 0;

        async Coroutine subTwo()
        {
            await Coroutine.Yield();

            count++;
        }

        async Coroutine subOne()
        {
            await subTwo();
        }

        async Coroutine main()
        {
            await subOne();
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        count.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Await_Nested_Coroutines_That_Fan_Out()
    {
        // Arrange
        var count = 0;

        async Coroutine sub2()
        {
            await Coroutine.Yield();
            count++;
        }

        async Coroutine sub1()
        {
            await sub2();
            await sub2();
        }

        async Coroutine main()
        {
            await sub1();
            await sub1();
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        count.Should().Be(4);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Await_Nested_Coroutines_That_Fan_Out()
    {
        // Arrange
        async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            return 1;
        }

        async Coroutine<int> sub1()
        {
            var r1 = await sub2();
            var r2 = await sub2();
            return r1 + r2;
        }

        async Coroutine<int> main()
        {
            var r1 = await sub1();
            var r2 = await sub1();
            return r1 + r2;
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(4);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Await_Nested_Coroutine()
    {
        // Arrange
        static async Coroutine<int> subTwo()
        {
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine<int> subOne() => await subTwo();

        static async Coroutine<int> main() => await subOne();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    public void Coroutine_Can_Await_Multiple_Nested_Coroutines(int depth)
    {
        // Arrange
        static async Coroutine main(int depth, int maxDepth)
        {
            if (depth < maxDepth)
            {
                await Coroutine.Yield();
                await main(depth + 1, maxDepth);
            }

            _ = false;
        }

        // Act
        var mainCo = main(0, depth);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    public void Coroutine_Of_Result_Can_Await_Multiple_Nested_Coroutines(int depth)
    {
        // Arrange
        static async Coroutine<int> main(int depth, int maxDepth)
        {
            if (depth < maxDepth)
            {
                await Coroutine.Yield();
                return await main(depth + 1, maxDepth);
            }

            return depth;
        }

        // Act
        var mainCo = main(0, depth);
        mainCo.ResumeUntil(
            c =>
            { return c.IsCompleted; });

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(depth);
    }

    [TestMethod]
    public void Coroutine_Cannot_Await_A_Coroutine_That_Has_No_Context()
    {
        // Arrange
        async Coroutine sub() => await Coroutine.Yield();

        var subCo = sub();
        subCo.ResumeUntil(c => !c.HasContext);

        async Coroutine main() => await subCo;

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result__Cannot_Await_A_Coroutine_That_Has_No_Context()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var subCo = sub();
        subCo.ResumeUntil(c => !c.HasContext);

        async Coroutine<int> main() => await subCo;

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Faulted_Sub_Faults_Awaiting()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine sub()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine main() => await sub();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsFaulted.Should().BeTrue();
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Faulted_Sub_Faults_Awaiting()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        async Coroutine<int> main() => await sub();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsFaulted.Should().BeTrue();
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Faulted_Sync_Sub_Throws()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine sub() => throw expectedException;

        async Coroutine main() => await sub();

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<Exception>().Subject.Should().Contain(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_Faulted_Sync_Sub_Throws()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine<int> sub() => throw expectedException;

        async Coroutine<int> main() => await sub();

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<Exception>().Subject.Should().Contain(expectedException);
    }

    [TestMethod]
    public void Coroutine_Can_Await_Sub_Of_Result()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine main() => await sub();

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Can_Await_Sub()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        static async Coroutine<int> main()
        {
            await sub();
            return 1;
        }

        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Cannot_Await_Awaited()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        var subCo1 = sub();

        async Coroutine main() => await subCo1;

        _ = main();

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<AwaitedCoroutineException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cannot_Await_Awaited()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var subCo1 = sub();

        async Coroutine<int> main() => await subCo1;

        _ = main();

        // Act
        var act = () => main();

        // Assert
        act.Should().Throw<AwaitedCoroutineException>();
    }
}