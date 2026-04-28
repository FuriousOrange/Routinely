namespace Routinely.UnitTests;

[TestClass]
public class CoroutineWhenAllTests : CoroutineTestBase
{
    [TestMethod]
    public void WhenAll_Null_Coroutines()
    {
        // Act
        var act = () => Coroutine.WhenAll(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void WhenAll_Empty_Coroutines()
    {
        // Act
        var act = () => Coroutine.WhenAll([]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Array_Coroutine_Results_Length_Match()
    {
        // Act
        var act = () => Coroutine.WhenAll(new Coroutine<int>[10], new int[5]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }


    [TestMethod]
    public void WhenAll_Of_Result_IEnumerable_Coroutine_Results_Length_Match()
    {
        // Act
        var list = new List<Coroutine<int>>()
        {
            Coroutine.FromResult(1),
            Coroutine.FromResult(1),
            Coroutine.FromResult(1),
        };

        var act = () => Coroutine.WhenAll(list, new int[5]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public async Task WhenAll_Completes_When_All_Complete()
    {
        // Arrange
        var completed1 = false;

        async Coroutine sub1()
        {
            await Coroutine.Yield();
            completed1 = true;
        }

        var subCo1 = sub1();

        var completed2 = false;

        async Coroutine sub2()
        {
            await Coroutine.Yield();
            await Coroutine.Yield();
            await Coroutine.Yield();
            completed2 = true;
        }

        var subCo2 = sub2();

        var mainCo = Coroutine.WhenAll(subCo1, subCo2);

        // Act
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        completed1.Should().BeTrue();
        completed2.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Completes_When_All_Complete()
    {
        // Arrange
        static async Coroutine<int> sub(int value)
        {
            await Coroutine.Yield();

            return value;
        }

        var subCo1 = sub(1);

        var subCo2 = sub(2);

        var mainCo = Coroutine.WhenAll(subCo1, subCo2);

        // Act
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result[0].Should().Be(1);
        mainCo.Result[1].Should().Be(2);
    }

    [TestMethod]
    public void WhenAll_Awaited_No_Context_Throws()
    {
        // Arrange
        async Coroutine noContext()
        {
            await Coroutine.Yield();
        }

        var noContextCo = noContext();
        noContextCo.ResumeUntil(c => !c.HasContext);

        // Act
        var act = () => Coroutine.WhenAll(noContextCo);

        // Assert
        act.Should().Throw<NoContextException>();

    }

    [TestMethod]
    public void WhenAll_Of_Result_Awaited_No_Context_Throws()
    {
        // Arrange
        async Coroutine<int> noContext()
        {
            await Coroutine.Yield();
            return 1;
        }

        var noContextCo = noContext();
        noContextCo.ResumeUntil(c => !c.HasContext);

        // Act
        var act = () => Coroutine.WhenAll(noContextCo);

        // Assert
        act.Should().Throw<NoContextException>();

    }

    [TestMethod]
    public void WhenAll_Faulted_Awaited_Throws()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine sub()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        var mainCo = Coroutine.WhenAll(sub());

        // Act
        var act = () => { while (Coroutine.ResumeAll()) ; };

        // Assert
        act.Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().Contain(expectedException);
    }

    [TestMethod]
    public void WhenAll_Of_Result_Faulted_Awaited_Throws()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        var mainCo = Coroutine.WhenAll(sub());

        // Act
        var act = () => { while (Coroutine.ResumeAll()) ; };

        // Assert
        act.Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().Contain(expectedException);
    }

    [TestMethod]
    public void WhenAll_Canceled_Awaiter_Cancels_Awaiteds()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        var subCo1 = sub();

        var subCo2 = sub();

        var mainCo = Coroutine.WhenAll(subCo1, subCo2);

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        subCo1.IsCanceled.Should().BeTrue();
        subCo2.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Canceled_Awaiter_Cancels_Awaiteds()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var subCo1 = sub();

        var subCo2 = sub();

        var mainCo = Coroutine.WhenAll(subCo1, subCo2);

        // Act
        mainCo.Cancel();

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        subCo1.IsCanceled.Should().BeTrue();
        subCo2.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Array_Passing_Results_Array()
    {
        // Arrange
        int[] results = new int[2];

        static async Coroutine<int> sub(int value)
        {
            for (var i = 0; i < value; i++)
            {
                await Coroutine.Yield();
            }
            return value;
        }

        var subCo1 = sub(1);
        var subCo2 = sub(2);

        var mainCo = Coroutine.WhenAll([subCo1, subCo2], results);

        // Act
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        results[0].Should().Be(1);
        results[1].Should().Be(2);
    }

    [TestMethod]
    public void WhenAll_Passing_IEnumerable()
    {
        // Arrange
        static async Coroutine sub() => await Coroutine.Yield();

        List<Coroutine> cos = [sub(), sub(), sub()];


        // Act
        var mainCo = Coroutine.WhenAll(cos);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_IEnumerable()
    {
        // Arrange
        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var cos = new List<Coroutine<int>> { sub(), sub(), sub() };


        // Act
        var mainCo = Coroutine.WhenAll(cos);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_IEnumerable_Passing_Results_Array()
    {
        // Arrange
        int[] results = new int[3];

        static async Coroutine<int> sub()
        {
            await Coroutine.Yield();
            return 1;
        }

        var cos = new List<Coroutine<int>> { sub(), sub(), sub() };


        // Act
        var mainCo = Coroutine.WhenAll(cos, results);
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        results[0].Should().Be(1);
        results[1].Should().Be(1);
        results[2].Should().Be(1);
    }

    [TestMethod]
    public void WhenAll_Awaited_Coroutine_Throws()
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
        var act = () => Coroutine.WhenAll(subCo1, subCo2);

        // Assert
        act.Should().Throw<AwaitedCoroutineException>()
            .And.AwaitedCoroutine.Should().Be(subCo2);
    }

    [TestMethod]
    public void WhenAll_Of_Result_Awaited_Coroutine_Throws()
    {
        // Arrange
        async Coroutine<int> sub2()
        {
            await Coroutine.Yield();
            return 2;
        }

        var subCo2 = sub2();

        async Coroutine<int> sub1()
        {
            await subCo2;
            return 1;
        }

        var subCo1 = sub1();

        // Act
        var act = () => Coroutine.WhenAll(subCo1, subCo2);

        // Assert
        act.Should().Throw<AwaitedCoroutineException>()
            .And.AwaitedCoroutine.Should().Be(subCo2);
    }

    [TestMethod]
    public void WhenAll_Sync_Completion()
    {
        // Arrange
        static async Coroutine sub() { /* sync */ }
        ;

        // Act
        var mainCo = Coroutine.WhenAll(sub(), sub());

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Sync_Completion()
    {
        // Arrange
        static async Coroutine<int> sub(int value) => value;

        // Act
        var mainCo = Coroutine.WhenAll(sub(1), sub(2));

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result[0].Should().Be(1);
        mainCo.Result[1].Should().Be(2);
    }

    [TestMethod]
    public void WhenAll_Sync_Async_Mixed_Completion()
    {
        // Arrange
        async Coroutine syncSub() { /* sync */ }
        async Coroutine asyncSub() => await Coroutine.Yield();

        // Act
        var mainCo = Coroutine.WhenAll(syncSub(), asyncSub());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void WhenAll_Of_Result_Sync_Async_Mixed_Completion()
    {
        // Arrange
        async Coroutine<int> syncSub() => 1; /* sync */
        async Coroutine<int> asyncSub() { await Coroutine.Yield(); return 2; }

        // Act
        var mainCo = Coroutine.WhenAll(syncSub(), asyncSub());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.Result[0].Should().Be(1);
        mainCo.Result[1].Should().Be(2);
    }

    [TestMethod]
    public void WhenAll_Completed_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine fault()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        var completedCo = Coroutine.CompletedCoroutine;
        var faultedCo = fault();

        // Act
        var mainCo = Coroutine.WhenAll(completedCo, faultedCo);
        var act = () => DrainCoroutines(@throw: true);

        // Assert
        act.Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().Contain(expectedException);
    }

    [TestMethod]
    public void WhenAll_Of_Result_Completed_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Coroutine<int> fault()
        {
            await Coroutine.Yield();
            throw expectedException;
        }

        var completedCo = Coroutine.FromResult(1);
        var faultedCo = fault();

        // Act
        var mainCo = Coroutine.WhenAll(completedCo, faultedCo);
        var act = () => DrainCoroutines(@throw: true);

        // Assert
        act.Should().Throw<AggregateException>()
            .And.InnerExceptions.Should().Contain(expectedException);
    }
}
