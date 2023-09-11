using System.Net;
using JsonApiDotNetCore;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

/// <inheritdoc />
public sealed class IdempotencyProvider : IIdempotencyProvider
{
    private readonly IdempotencyDbContext _dbContext;
    private readonly ISystemClock _systemClock;
    private readonly IOperationsTransactionFactory _transactionFactory;

    public IdempotencyProvider(IdempotencyDbContext dbContext, ISystemClock systemClock, IOperationsTransactionFactory transactionFactory)
    {
        ArgumentGuard.NotNull(dbContext, nameof(dbContext));
        ArgumentGuard.NotNull(systemClock, nameof(systemClock));
        ArgumentGuard.NotNull(transactionFactory, nameof(transactionFactory));

        _dbContext = dbContext;
        _systemClock = systemClock;
        _transactionFactory = transactionFactory;
    }

    /// <inheritdoc />
    public bool IsSupported(HttpRequest request)
    {
        return request.Method == HttpMethod.Post.Method && !request.RouteValues.ContainsKey("relationshipName");
    }

    /// <inheritdoc />
    public async Task<IdempotentResponse?> GetResponseFromCacheAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        RequestCacheItem? cacheItem = await _dbContext.RequestCache.FirstOrDefaultAsync(item => item.Id == idempotencyKey, cancellationToken);

        if (cacheItem == null)
        {
            return null;
        }

        if (cacheItem.ResponseStatusCode == null)
        {
            // Unlikely, but depending on the transaction isolation level, we may observe this uncommitted intermediate state.
            throw CreateErrorForConcurrentRequest(idempotencyKey);
        }

        return new IdempotentResponse(cacheItem.RequestFingerprint, cacheItem.ResponseStatusCode.Value, cacheItem.ResponseLocationHeader,
            cacheItem.ResponseContentTypeHeader, cacheItem.ResponseBody);
    }

    private static JsonApiException CreateErrorForConcurrentRequest(string idempotencyKey)
    {
        return new JsonApiException(new ErrorObject(HttpStatusCode.Conflict)
        {
            Title = $"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.",
            Detail = $"The request for the provided idempotency key '{idempotencyKey}' is currently being processed.",
            Source = new ErrorSource
            {
                Header = HeaderConstants.IdempotencyKey
            }
        });
    }

    /// <inheritdoc />
    public async Task<IOperationsTransaction> BeginRequestAsync(string idempotencyKey, string requestFingerprint, CancellationToken cancellationToken)
    {
        try
        {
            IOperationsTransaction transaction = await _transactionFactory.BeginTransactionAsync(cancellationToken);

            var cacheItem = new RequestCacheItem(idempotencyKey, requestFingerprint, _systemClock.UtcNow);
            await _dbContext.RequestCache.AddAsync(cacheItem, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return transaction;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw CreateErrorForConcurrentRequest(idempotencyKey);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }

    public async Task CompleteRequestAsync(string idempotencyKey, IdempotentResponse response, IOperationsTransaction transaction,
        CancellationToken cancellationToken)
    {
        RequestCacheItem cacheItem = await _dbContext.RequestCache.FirstAsync(item => item.Id == idempotencyKey, cancellationToken);

        cacheItem.ResponseStatusCode = response.ResponseStatusCode;
        cacheItem.ResponseLocationHeader = response.ResponseLocationHeader;
        cacheItem.ResponseContentTypeHeader = response.ResponseContentTypeHeader;
        cacheItem.ResponseBody = response.ResponseBody;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
