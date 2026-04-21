namespace Routinely.UnitTests;

[TestClass]
public class CoroutineContextTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_ResumeAll_Multiple_Contexts()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine context1Main() => await Coroutine.Yield();

        Coroutine.SetContext(context1);
        var context1Co = context1Main();

        async Coroutine context2Main() => await Coroutine.Yield();

        Coroutine.SetContext(context2);
        var context2Co = context2Main();

        // Act
        Coroutine.SetContext(context1);
        while (context1Co.IsCompleted == false && Coroutine.ResumeAll()) ;

        // Assert
        context1Co.IsCompleted.Should().BeTrue();
        context2Co.IsCompleted.Should().BeFalse();

        // Act
        Coroutine.SetContext(context2);
        while (context2Co.IsCompleted == false && Coroutine.ResumeAll()) ;

        // Assert
        context2Co.IsCompleted.Should().BeTrue();
        context1Co.HasContext.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Migrate_Between_Contexts_Sequentially_From_Current_Context()
    {
        // Arrange
        var context1Complete = false;
        var context2Complete = false;

        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();


        async Coroutine main()
        {
            await Coroutine.Context(context1);

            await Coroutine.Yield();

            context1Complete = true;

            await Coroutine.Context(context2);

            await Coroutine.Yield();

            context2Complete = true;
        }

        // Act
        Coroutine.SetContext(context1);
        var mainCo = main();
        mainCo.ResumeUntil(c => context1Complete);

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        context1Complete.Should().BeTrue();
        context2Complete.Should().BeFalse();

        // Act

        // Resume the first context a few times to ensure we've switched context and mainCo doesn't complete
        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();
        }

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        context1Complete.Should().BeTrue();
        context2Complete.Should().BeFalse();

        // Act
        Coroutine.SetContext(context2); // Swap to second context and resume until completion
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        context1Complete.Should().BeTrue();
        context2Complete.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Declared_Context_Overrides_Caller_Context()
    {
        // Critical test: Proves that a coroutine can force it's own context, even if the caller is on a different context.
        // This is important for ensuring that coroutines can be safely declared on one context, but then used on another without accidentally running on the wrong context.
        
        // Arrange
        var context1Complete = false;

        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();


        async Coroutine main()
        {
            await Coroutine.Context(context1);

            await Coroutine.Yield();

            context1Complete = true;
        }

        // Act
        Coroutine.SetContext(context2);
        var mainCo = main(); // Create from context2, but first thing it does is switch to context1

        for (var i = 0; i < 100000; i++)
        {
            Coroutine.ResumeAll();
        }

        // Assert
        mainCo.IsCompleted.Should().BeFalse();
        context1Complete.Should().BeFalse();

        // Act
        Coroutine.SetContext(context1); // Swap to first context and resume until completion
        mainCo.ResumeUntil(c => context1Complete);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        context1Complete.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_SwitchTo_Maintains_Coroutine_Context()
    {
        // Critical test: Proves that switching to a coroutine that changes it's context from the currently resuming context
        // doesn't stay on the old context when a switch to coroutine forces a context change.

        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine switchToContext1()
        {
            await Coroutine.Context(context1);
        }

        async Coroutine setContext2()
        {
            await Coroutine.Context(context2);
            await Coroutine.SwitchTo(switchToContext1);
        }

        async Coroutine main() => await setContext2();

        // Act
        Coroutine.SetContext(context1);
        var mainCo = main();

        Coroutine.SetContext(context2);
        Coroutine.ResumeAll(); // Switch to
        Coroutine.ResumeAll(); // Coroutine should be in context 1
        Coroutine.SetContext(context1);

        // Assert
        context1.StackCount.Should().Be(1);
        context1.Stacks[0].HeadIndex.Should().NotBe(0);
        context2.StackCount.Should().Be(0);
        context2.Stacks[0].Should().BeNull();
    }

    [TestMethod]
    public void Coroutine_SwitchTo_With_Context_Maintains_Coroutine_Context()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine switchToContext1()
        {
            await Coroutine.Context(context1);
        }

        async Coroutine setContext2()
        {
            await Coroutine.Context(context2);
            await Coroutine.SwitchTo(this, @this => switchToContext1());
        }

        async Coroutine main() => await setContext2();

        // Act
        Coroutine.SetContext(context1);
        var mainCo = main();

        Coroutine.SetContext(context2);
        Coroutine.ResumeAll(); // Switch to
        Coroutine.ResumeAll(); // Coroutine should be in context 1
        Coroutine.SetContext(context1);

        // Assert
        context1.StackCount.Should().Be(1);
        context1.Stacks[0].HeadIndex.Should().NotBe(0);
        context2.StackCount.Should().Be(0);
        context2.Stacks[0].Should().BeNull();
    }

    [TestMethod]
    public void Coroutine_After_Migration_Swapped_From_Original_Context_Executes()
    {
        // Critical test: Proves that when a coroutine migrates between contexts
        // the coroutine that replaces it in the dispatcher is executed instead of skipped

        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine first()
        {
            await Coroutine.Yield();
            await Coroutine.Context(context2); // Forces yield here
        }

        async Coroutine  second()
        {
            await Coroutine.Yield();
        }

        // Act
        var firstCo = first();
        var secondCo = second();

        Coroutine.ResumeAll(); // firstCo switches context, secondCo resumes after yield and completes

        // Assert
        secondCo.IsCompleted.Should().BeTrue();
    }
}
