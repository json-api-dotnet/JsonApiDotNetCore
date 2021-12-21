using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

// Based on https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
[PublicAPI]
public sealed class AsyncAutoResetEvent
{
    private static readonly Task CompletedTask = Task.FromResult(true);

    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private bool _isSignaled;

    public Task WaitAsync()
    {
        lock (_waiters)
        {
            if (_isSignaled)
            {
                _isSignaled = false;
                return CompletedTask;
            }

            var source = new TaskCompletionSource<bool>();
            _waiters.Enqueue(source);
            return source.Task;
        }
    }

    public void Set()
    {
        TaskCompletionSource<bool>? sourceToRelease = null;

        lock (_waiters)
        {
            if (_waiters.Count > 0)
            {
                sourceToRelease = _waiters.Dequeue();
            }
            else if (!_isSignaled)
            {
                _isSignaled = true;
            }
        }

        sourceToRelease?.SetResult(true);
    }
}
