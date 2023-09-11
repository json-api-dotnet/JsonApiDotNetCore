namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

/// <summary>
/// Helps to coordinate between API server and test client, with the goal of producing a concurrency conflict.
/// </summary>
public sealed class TestExecutionMediator
{
    private readonly AsyncAutoResetEvent _serverNotifyEvent = new();

    /// <summary>
    /// Used by the server to notify the test client that the request being processed has entered a transaction. After notification, this method blocks for
    /// the duration of <paramref name="sleepTime" /> to allow the test client to start a second request (and block when entering its own transaction), while
    /// the current request is still running.
    /// </summary>
    internal async Task NotifyTransactionStartedAsync(TimeSpan sleepTime, CancellationToken cancellationToken)
    {
        _serverNotifyEvent.Set();

        await Task.Delay(sleepTime, cancellationToken);
    }

    /// <summary>
    /// Used by the test client to wait until the server request being processed has entered a transaction.
    /// </summary>
    internal async Task WaitForTransactionStartedAsync(TimeSpan timeout)
    {
        Task task = _serverNotifyEvent.WaitAsync();
        await TimeoutAfterAsync(task, timeout);
    }

    private static async Task TimeoutAfterAsync(Task task, TimeSpan timeout)
    {
        // Based on https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#using-a-timeout

        if (timeout != TimeSpan.Zero)
        {
            using var timerCancellation = new CancellationTokenSource();
            Task timeoutTask = Task.Delay(timeout, timerCancellation.Token);

            Task firstCompletedTask = await Task.WhenAny(task, timeoutTask);

            if (firstCompletedTask == timeoutTask)
            {
                throw new TimeoutException();
            }

            // The timeout did not elapse, so cancel the timer to recover system resources.
            timerCancellation.Cancel();
        }

        // Re-throw any exceptions from the completed task.
        await task;
    }
}
