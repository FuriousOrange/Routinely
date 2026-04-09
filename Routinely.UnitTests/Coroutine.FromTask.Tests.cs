namespace Routinely.UnitTests;

[TestClass]
public class CoroutineFromTaskTests : CoroutineTestBase
{
    [TestMethod]
    public void Coroutine_From_Task_Sync_Completed_Task()
    {
        // Arrange
        static async Task main() { /* sync */ }

        // Act
        var mainCo = Coroutine.FromTask(main());

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_From_Task_Of_Result_Sync_Completed_Task()
    {
        // Arrange
        static async Task<int> main() => 1;

        // Act
        var mainCo = Coroutine.FromTask(main());

        // Assert
        mainCo.IsCompleted.Should().BeTrue();

    }

    [TestMethod]
    public void Coroutine_From_Task_Async_Completed_Task()
    {
        // Arrange
        var completed = false;


        async Task main()
        {
            await Task.Delay(100);
            completed = true;
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        completed.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_TResult_From_Task_Async_Completed_Task()
    {
        // Arrange
        async Task<int> main()
        {
            await Task.Delay(100);
            return 1;
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        mainCo.ResumeUntil(c => c.IsCompleted);

        // Assert
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }

    [TestMethod]
    public void Coroutine_From_Task_Faulted_Task()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Task main()
        {
            await Task.Delay(100);
            throw expectedException;
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_Task_Faulted_Task()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Task<int> main()
        {
            await Task.Delay(100);
            throw expectedException;
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_From_Task_Canceled_Task_Faults_Coroutine()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        async Task main()
        {
            while (true)
            {
                await Task.Yield();
                token.ThrowIfCancellationRequested();
            }
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        cancellationSource.Cancel();
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_Task_Canceled_Task_Faults_Coroutine()
    {
        // Arrange
        var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        async Task<int> main()
        {
            while (true)
            {
                try
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // Act
        var mainCo = Coroutine.FromTask(main());
        cancellationSource.Cancel();
        mainCo.ResumeUntil(c => c.IsFaulted);

        // Assert
        mainCo.Exception.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void Coroutine_From_Task_With_CancellationToken_Cancel_Coroutine_Cancels_Task()
    {
        // Arrange
        var taskCanceled = false;

        async Task main(CancellationToken token)
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
        var mainCo = Coroutine.FromTask(t => main(t));
        mainCo.Cancel();

        while (!taskCanceled) { } // Spin here until the task observes the cancellation and sets the flag, which ensures that the task has actually been canceled before we assert on the coroutine state

        // Assert
        mainCo.IsCanceled.Should().BeTrue();
        taskCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_Task_With_CancellationToken_Cancel_Coroutine_Cancels_Task()
    {
        // Arrange
        var taskCanceled = false;
 
        async Task<int> main(CancellationToken token)
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
        var mainCo = Coroutine.FromTask(t => main(t));
        mainCo.Cancel();

        while (!taskCanceled) { } // Spin here until the task observes the cancellation and sets the flag, which ensures that the task has actually been canceled before we assert on the coroutine state
        
        // Assert
        mainCo.IsCanceled.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_From_Task_Sync_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Task main()
        {
            throw expectedException;
        }

        // Act
        var act = () => Coroutine.FromTask(main());

        // Assert
        act.Should().Throw<Exception>().Which.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_Task_Sync_Faulted()
    {
        // Arrange
        var expectedException = new Exception("Boom!");

        async Task<int> main()
        {
            throw expectedException;
        }

        // Act
        var act = () => Coroutine.FromTask(main());

        // Assert
        act.Should().Throw<Exception>().Which.Should().Be(expectedException);
    }

    [TestMethod]
    public void Coroutine_From_Task_With_CancellationToken_Sync_Completed()
    {
        // Arrange
        static async Task main(CancellationToken token) { /* sync */ }

        // Act
        var mainCo = Coroutine.FromTask(t => main(t));

        // Assert
        mainCo.ResumeUntil(c => c.IsCompleted);
        mainCo.IsCompleted.Should().BeTrue();
    }

    [TestMethod]
    public void Coroutine_Of_Result_From_Task_With_CancellationToken_Sync_Completed()
    {
        // Arrange
        static async Task<int> main(CancellationToken token) => 1;

        // Act
        var mainCo = Coroutine.FromTask(t => main(t));

        // Assert
        mainCo.ResumeUntil(c => c.IsCompleted);
        mainCo.IsCompleted.Should().BeTrue();
        mainCo.Result.Should().Be(1);
    }
}
