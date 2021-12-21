using JsonApiDotNetCore.AtomicOperations;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware;

internal sealed class NoIdempotencyProvider : IIdempotencyProvider
{
    /// <inheritdoc />
    public bool IsSupported(HttpRequest request)
    {
        return false;
    }

    /// <inheritdoc />
    public Task<IdempotentResponse?> GetResponseFromCacheAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IOperationsTransaction> BeginRequestAsync(string idempotencyKey, string requestFingerprint, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task CompleteRequestAsync(string idempotencyKey, IdempotentResponse response, IOperationsTransaction transaction,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
