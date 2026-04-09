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
}
