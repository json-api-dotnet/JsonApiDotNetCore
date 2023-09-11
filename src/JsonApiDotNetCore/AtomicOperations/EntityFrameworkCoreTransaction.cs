using JetBrains.Annotations;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.AtomicOperations;

/// <summary>
/// Represents an Entity Framework Core transaction in an atomic:operations request.
/// </summary>
[PublicAPI]
public sealed class EntityFrameworkCoreTransaction : IOperationsTransaction
{
    private readonly IDbContextTransaction _transaction;
    private readonly DbContext _dbContext;
    private readonly bool _ownsTransaction;

    /// <inheritdoc />
    public string TransactionId => _transaction.TransactionId.ToString();

    public EntityFrameworkCoreTransaction(IDbContextTransaction transaction, DbContext dbContext, bool ownsTransaction)
    {
        ArgumentGuard.NotNull(transaction);
        ArgumentGuard.NotNull(dbContext);

        _transaction = transaction;
        _dbContext = dbContext;
        _ownsTransaction = ownsTransaction;
    }

    /// <summary>
    /// Detaches all entities from the Entity Framework Core change tracker.
    /// </summary>
    public Task BeforeProcessOperationAsync(CancellationToken cancellationToken)
    {
        _dbContext.ResetChangeTracker();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public Task AfterProcessOperationAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_ownsTransaction)
        {
            await _transaction.CommitAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_ownsTransaction)
        {
            await _transaction.DisposeAsync();
        }
    }
}
