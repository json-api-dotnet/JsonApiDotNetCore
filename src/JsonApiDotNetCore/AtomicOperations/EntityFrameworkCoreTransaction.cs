using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Represents an Entity Framework Core transaction in an atomic:operations request.
    /// </summary>
    [PublicAPI]
    public sealed class EntityFrameworkCoreTransaction : IOperationsTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private readonly DbContext _dbContext;

        /// <inheritdoc />
        public string TransactionId => _transaction.TransactionId.ToString();

        public EntityFrameworkCoreTransaction(IDbContextTransaction transaction, DbContext dbContext)
        {
            ArgumentGuard.NotNull(transaction, nameof(transaction));
            ArgumentGuard.NotNull(dbContext, nameof(dbContext));

            _transaction = transaction;
            _dbContext = dbContext;
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
