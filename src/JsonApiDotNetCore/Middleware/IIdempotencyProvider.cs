using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware;

[PublicAPI]
public interface IIdempotencyProvider
{
    /// <summary>
    /// Indicates whether the current request supports idempotency.
    /// </summary>
    bool IsSupported(HttpRequest request);

    /// <summary>
    /// Looks for a matching response in the idempotency cache for the specified idempotency key.
    /// </summary>
    Task<IdempotentResponse?> GetResponseFromCacheAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new cache entry inside a transaction, so that concurrent requests with the same idempotency key will block or fail while the transaction
    /// hasn't been committed.
    /// </summary>
    Task<IOperationsTransaction> BeginRequestAsync(string idempotencyKey, string requestFingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the produced response in the cache and commits its transaction.
    /// </summary>
    Task CompleteRequestAsync(string idempotencyKey, IdempotentResponse response, IOperationsTransaction transaction, CancellationToken cancellationToken);
}
