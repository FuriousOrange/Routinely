namespace Routinely.UnitTests;

[TestClass]
public class CoroutineThreadTests : CoroutineThreadTestBase
{
    [TestMethod]
    public void Coroutine_Cannot_Await_A_Cross_Thread_Coroutine()
    {
        // Arrange
        async Coroutine crossMain(Coroutine main)
        {
            await main;
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        var mainCo = main();

        // Act
        AddWork(() => crossMain(mainCo).Forget());
        var act = () => StartThread();

        // Assert
        act.Should().Throw<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_Cannot_Await_A_Cross_Thread_Coroutine()
    {
        // Arrange
        async Coroutine<int> crossMain(Coroutine<int> main)
        {
            await main;
            return 1;
        }

        async Coroutine<int> main()
        {
            await Coroutine.Yield();
            return 1;
        }

        var mainCo = main();

        // Act
        AddWork(() => crossMain(mainCo).Forget());
        var act = () => StartThread();

        // Assert
        act.Should().Throw<NoContextException>();
    }

    [TestMethod]
    public void Coroutine_Cannot_Migrate_Cross_Thread_Context()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();

        Coroutine.SetContext(context1);

        async Coroutine crossMain(CoroutineContext context)
        {
            await Coroutine.Context(context);
        }

        // Act
        AddWork(() => crossMain(context1).Forget());
        var act = () => StartThread();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }


    [TestMethod]
    public void Coroutine_Cannot_Set_Context_Cross_Thread()
    {
        // Arrange
        var context1 = Coroutine.CreateContext();

        Coroutine.SetContext(context1);

        async Coroutine crossMain(Coroutine main)
        {
            main.SetContext(context1);
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        var mainCo = main();

        // Act
        AddWork(() => crossMain(mainCo).Forget());
        var act = () => StartThread();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void Coroutine_Cannot_Cross_Thread_Cancel()
    {
        // Arrange
        async Coroutine crossMain(Coroutine main)
        {
            main.Cancel();
        }

        async Coroutine main()
        {
            await Coroutine.Yield();
        }

        // Act
        var mainCo = main();
        AddWork(() => crossMain(mainCo).Forget());
        var act = () => StartThread(); 
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // TODO : Test SwitchTo a delegate that returns a coroutine from another thread
}
