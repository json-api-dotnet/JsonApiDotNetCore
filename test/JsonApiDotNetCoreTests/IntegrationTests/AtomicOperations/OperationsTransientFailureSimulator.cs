using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

/// <summary>
/// Used to simulate transient database failures during <see cref="EntityFrameworkCoreRepository{TResource,TId}.SaveChangesAsync" />.
/// </summary>
public sealed class OperationsTransientFailureSimulator
{
    internal bool FailOnNextAttempt { get; set; }
    internal bool UseTransientError { get; set; }
    internal int AttemptCount { get; private set; }

    internal void Reset()
    {
        FailOnNextAttempt = false;
        UseTransientError = false;
        AttemptCount = 0;
    }

    internal void FailAsConfigured()
    {
        AttemptCount++;

        if (FailOnNextAttempt)
        {
            FailOnNextAttempt = false;

            Exception innerException = UseTransientError
                ? new TimeoutException("Simulated transient database timeout.")
                : new InvalidOperationException("Simulated non-transient database failure.");

            throw new DbUpdateException("Simulated database failure.", innerException);
        }
    }
}
