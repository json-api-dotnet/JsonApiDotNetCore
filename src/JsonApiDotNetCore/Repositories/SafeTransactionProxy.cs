using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Gets the current transaction or creates a new one.
    /// If a transaction already exists, commit, rollback and dispose
    /// will not be called. It is assumed the creator of the original
    /// transaction should be responsible for disposal.
    /// </summary>
    internal class SafeTransactionProxy : IDbContextTransaction
    {
        private readonly bool _shouldExecute;
        private readonly IDbContextTransaction _transaction;

        private SafeTransactionProxy(IDbContextTransaction transaction, bool shouldExecute)
        {
            _transaction = transaction;
            _shouldExecute = shouldExecute;
        }

        public static async Task<IDbContextTransaction> GetOrCreateAsync(DatabaseFacade databaseFacade)
            => databaseFacade.CurrentTransaction != null
                ? new SafeTransactionProxy(databaseFacade.CurrentTransaction, shouldExecute: false)
                : new SafeTransactionProxy(await databaseFacade.BeginTransactionAsync(), shouldExecute: true);

        /// <inheritdoc />
        public Guid TransactionId => _transaction.TransactionId;

        /// <inheritdoc />
        public void Commit() => Proxy(t => t.Commit());
        
        /// <inheritdoc />
        public void Rollback() => Proxy(t => t.Rollback());
        
        /// <inheritdoc />
        public void Dispose() => Proxy(t => t.Dispose());

        private void Proxy(Action<IDbContextTransaction> func)
        {
            if(_shouldExecute) 
                func(_transaction);
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
