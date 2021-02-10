using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Represents an Entity Framework Core transaction in an atomic:operations request.
    /// </summary>
    public sealed class EntityFrameworkCoreTransaction : IOperationsTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private readonly DbContext _dbContext;

        /// <inheritdoc />
        public Guid TransactionId => _transaction.TransactionId;

        public EntityFrameworkCoreTransaction(IDbContextTransaction transaction, DbContext dbContext)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
