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
        firstCo.IsCompleted.Should().BeFalse();
        secondCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Can_Be_Cancelled_After_Migrating()
    {
        // Critical test: Proves that after a coroutine migrates it's stack integrity is persisted for the coruoutine handle

        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine main()
        {
            await Coroutine.Context(context2);
        }

        // Act
        var mainCo = main();
        Coroutine.ResumeAll(); // Resume to migrate to context2

        mainCo.Cancel();

        // Assert
        context1.StackCount.Should().Be(0);
        context2.StackCount.Should().Be(1);
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();

        mainCo.CoreToken.Should().Be(context2.Stacks[0].Tokens[0]); // Confirm token hasn't been lost during migration
    }

    [TestMethod]
    public void Coroutine_Can_Be_Cancelled_After_Migrating_Then_Switch_To()
    {
        // Critical test: Proves that after a coroutine migrates it's stack, switches to another coroutine
        // that migrates it's stack again, the original coroutine can still be cancelled

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

        mainCo.Cancel();

        // Assert
        context1.StackCount.Should().Be(1);
        context2.StackCount.Should().Be(0);
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.IsCanceled.Should().BeTrue();

        mainCo.CoreToken.Should().Be(context1.Stacks[0].Tokens[0]); // Confirm token hasn't been lost during migration
    }

    [TestMethod]
    public void Coroutine_Can_Be_Manually_Migrated()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();

        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        // Act
        Coroutine.SetContext(context1);
        var mainCo = main();
        mainCo.SetContext(context2);

        Coroutine.ResumeAll(); // Resume to ensure migration is processed

        // Assert
        context1.StackCount.Should().Be(0);
        context2.StackCount.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Be_Manually_Migrated_To_The_Same_Context()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();
 
        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        // Act
        Coroutine.SetContext(context1);
        var mainCo = main();
        var act = () => mainCo.SetContext(context1);

        // Assert
        act.Should().NotThrow();
        context1.StackCount.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Be_Manually_Migrated_Over_Multiple_Contexts()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();
        var context3 = Coroutine.CreateContext();

        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        // Act, Assert
        Coroutine.SetContext(context1);
        var mainCo = main();

        mainCo.SetContext(context1);
        Coroutine.ResumeAll(); // Resume to ensure migration is processed

        context1.StackCount.Should().Be(1);
        context2.StackCount.Should().Be(0);
        context3.StackCount.Should().Be(0);

        mainCo.SetContext(context2);
        Coroutine.ResumeAll(); // Resume to ensure migration is processed

        context1.StackCount.Should().Be(0);
        context2.StackCount.Should().Be(1);
        context3.StackCount.Should().Be(0);

        mainCo.SetContext(context3);
        Coroutine.ResumeAll(); // Resume to ensure migration is processed

        context1.StackCount.Should().Be(0);
        context2.StackCount.Should().Be(0);
        context3.StackCount.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_Can_Be_Manually_Migrated_Inside_Swtch_To_When_Captured()
    {
        // Critical test: Proves that when a coroutine migrates between contexts during a switch to,
        // the coroutine can still be manually migrated after the switch to without losing stack integrity

        // Arrange
        var context1 = Coroutine.CreateContext();
        var context2 = Coroutine.CreateContext();
        Coroutine mainCo = default;

        async Coroutine main()
        {
            await Coroutine.SwitchTo(manualSetContext);
        }

        async Coroutine manualSetContext()
        {
            // Capture mainCo here and force it to migrate manually (no await Coroutine.Context) to ensure that the stack integrity is maintained.
            // This is a valid use case for a switch to closing round the handle and interacting with it
            mainCo.SetContext(context2);
        }

        // Act
        Coroutine.SetContext(context1);
        mainCo = main();

        Coroutine.ResumeAll(); // Switch to manualSetContext, which will migrate the context of mainCo to context2
        Coroutine.ResumeAll(); // Resume to ensure migration is processed

        // Assert
        context1.StackCount.Should().Be(0);
        context2.StackCount.Should().Be(1);
    }
}
