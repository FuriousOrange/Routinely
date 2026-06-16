namespace Routinely.UnitTests;

[TestClass]
public class CoroutineWhenAnyTests : CoroutineTestBase
{
    [TestMethod]
    public void WhenAny_Null_Coroutines()
    {
        // Act
        Action act = () => Coroutine.WhenAny(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void WhenAny_Empty_Coroutines()
    {
        // Act
        var act = () => Coroutine.WhenAny(Array.Empty<Coroutine>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void WhenAny_Completes_When_One_Completes()
    {
        // Arrange
        static async Coroutine sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        static async Coroutine sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
    }

    [TestMethod]
    public void WhenAny_Of_Result_Completes_When_One_Completes()
    {
        // Arrange
        static async Coroutine<int> sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 2;
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
    }

    [TestMethod]
    public void WhenAny_Awaited_No_Context_Throws()
    {
        // Arrange
        async Coroutine noContext()
        {
            await Coroutine.Yield();
        }

        var noContextCo = noContext();
        noContextCo.ResumeUntil(c => !c.HasContext);

        // Act
        var act = () => Coroutine.WhenAny(noContextCo);

        // Assert
        act.Should().Throw<NoContextException>();

    }

    [TestMethod]
    public void WhenAny_Completes_When_One_Completes_And_Result_Can_Be_Accessed()
    {
        // Arrange
        static async Coroutine sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        static async Coroutine sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
        mainCo.Result.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAny_Of_Result_Completes_When_One_Completes_And_Result_Can_Be_Accessed()
    {
        // Arrange
        static async Coroutine<int> sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 2;
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
        mainCo.Result.IsCompleted.Should().BeTrue();
        mainCo.Result.Result.Should().Be(1);
    }

    [TestMethod]
    public void WhenAny_Completes_When_One_Completes_And_Result_Can_Be_Awaited()
    {
        // Arrange
        static async Coroutine sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        static async Coroutine sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        async Coroutine main()
        {
            var co = await Coroutine.WhenAny(subCo1, subCo2);
            await co;
        }


        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAny_Of_Result_Completes_When_One_Completes_And_Result_Can_Be_Awaited()
    {
        // Arrange
        static async Coroutine<int> sub1()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 1;
        }

        static async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 2;
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        async Coroutine<int> main()
        {
            var co = await Coroutine.WhenAny(subCo1, subCo2);
            return await co;
        }


        // Act
        var mainCo = main();
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void WhenAny_Completes_When_One_Faults()
    {
        // Arrange
        static async Coroutine sub1()
        {
            await Coroutine.Yield();
            throw new Exception("Boom!");
        }

        static async Coroutine sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
        }

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
    }

    [TestMethod]
    public void WhenAny_Of_Result_Completes_When_One_Faults()
    {
        // Arrange
        static async Coroutine<int> sub1()
        {
            await Coroutine.Yield();
            throw new Exception("Boom!");
        }

        var sub2 = async Coroutine<int> () =>
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
            return 2;
        };

        var subCo1 = sub1();
        var subCo2 = sub2();

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, subCo2);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result.Should().Be(subCo1);
    }

    [TestMethod]
    public void WhenAny_Sync_Completion()
    {
        // Arrange
        static async Coroutine sub1() => await Coroutine.Yield();

        var subCo1 = sub1();
        var completedCo = Coroutine.CompletedCoroutine;

        // Act
        var mainCo = Coroutine.WhenAny(subCo1, completedCo);

        // Assert
        mainCo.Result.Should().Be(completedCo);
    }

    [TestMethod]
    public void WhenAny_Awaited_Coroutine_Throws()
    {
        // Arrange
        async Coroutine sub2()
        {
            await Coroutine.Yield();
        }

        var subCo2 = sub2();

        async Coroutine sub1()
        {
            await subCo2;
        }

        var subCo1 = sub1();

        // Act
        var act = () => Coroutine.WhenAny(subCo1, subCo2);

        // Assert
        act.Should().Throw<AwaitedCoroutineException>()
            .And.AwaitedCoroutine.Should().Be(subCo2);
    }

    [TestMethod]
    public void WhenAny_Passing_IEnumerable()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        List<Coroutine> cos = [sub(), sub(), sub()];

        // Act
        var mainCo = Coroutine.WhenAny(cos);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        cos.Contains(mainCo.Result).Should().BeTrue();
    }

    [TestMethod]
    public void WhenAny_Of_Result_Passing_IEnumerable()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        List<Coroutine<int>> cos = [sub(), sub(), sub()];

        // Act
        var mainCo = Coroutine.WhenAny(cos);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        cos.Contains(mainCo.Result).Should().BeTrue();
    }

    [TestMethod]
    public void WhenAny_Canceled_Awaiter_Cancels_Awaiteds()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        var subCo1 = sub();

        var subCo2 = sub();

        var mainCo = Coroutine.WhenAny(subCo1, subCo2);

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        subCo1.IsCanceled.Should().BeTrue();
        subCo2.IsCanceled.Should().BeTrue();
    }
}