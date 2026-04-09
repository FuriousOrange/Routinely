namespace Routinely.UnitTests;

[TestClass]
public class CoroutineFromValueTaskTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_From_ValueTask_Sync_Completed()
    {
        // Arrange
        static async ValueTask main() { /* sync */ }

        // Act
        var mainCo = Coroutine.FromValueTask(main());

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_Of_Result_Sync_Completed()
    {
        // Arrange
        static async ValueTask<int> main() => 1;

        // Act
        var mainCo = Coroutine.FromValueTask(main());

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_Async_Completed()
    {
        // Arrange
        var completed = false;

        async ValueTask main()
        {
            await Task.Delay(100);
            completed = true;
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        completed.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_TResult_From_ValueTask_Async_Completed()
    {
        // Arrange
        async ValueTask<int> main()
        {
            await Task.Delay(100);
            return 1;
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async ValueTask main()
        {
            await Task.Delay(100);
            throw expectedException;
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_ValueTask_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async ValueTask<int> main()
        {
            await Task.Delay(100);
            throw expectedException;
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_Sync_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async ValueTask main()
        {
            throw expectedException;
        }

        // Act
        var act = () => Coroutine.FromValueTask(main());

        // Assert
        act.Should().Throw<Exception>().Which.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_ValueTask_Sync_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async ValueTask<int> main()
        {
            throw expectedException;
        }

        // Act
        var act = () => Coroutine.FromValueTask(main());

        // Assert
        act.Should().Throw<Exception>().Which.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_Canceled_Faults_Coroutine()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        async ValueTask main()
        {
            while (true)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
            }
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        cancellationSource.Cancel();
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_ValueTask_Canceled_Faults_Coroutine()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        async ValueTask<int> main()
        {
            while (true)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
            }
        }

        // Act
        var mainCo = Coroutine.FromValueTask(main());
        cancellationSource.Cancel();
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_With_CancellationToken_Cancel_Coroutine_Cancels_Task()
    {
        // Arrange
        var taskCanceled = false;

        async ValueTask main(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }
                catch
                {
                    taskCanceled = true;
                    throw;
                }
            }
        }

        // Act
        var mainCo = Coroutine.FromValueTask(t => main(t));
        mainCo.Cancel();

        while (!taskCanceled) { } // Spin until task observes cancellation

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        taskCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_ValueTask_With_CancellationToken_Cancel_Coroutine_Cancels_Task()
    {
        // Arrange
        var taskCanceled = false;

        async ValueTask<int> main(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }
                catch
                {
                    taskCanceled = true;
                    throw;
                }
            }
        }

        // Act
        var mainCo = Coroutine.FromValueTask(t => main(t));
        mainCo.Cancel();

        while (!taskCanceled) { } // Spin until task observes cancellation

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        taskCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_From_ValueTask_With_CancellationToken_Sync_Completed()
    {
        // Arrange
        static async ValueTask main(CancellationToken token) { /* sync */ }

        // Act
        var mainCo = Coroutine.FromValueTask(t => main(t));

        // Assert
        mainCo.ResumeUntil(c => c.IsCompleted);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_ValueTask_With_CancellationToken_Sync_Completed()
    {
        // Arrange
        static async ValueTask<int> main(CancellationToken token) => 1;

        // Act
        var mainCo = Coroutine.FromValueTask(t => main(t));

        // Assert
        mainCo.ResumeUntil(c => c.IsCompleted);
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }
}