using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.Repositories
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// If not already tracked, attaches the specified resource to the change tracker in <see cref="EntityState.Unchanged"/> state.
        /// </summary>
        public static IIdentifiable GetTrackedOrAttach(this DbContext dbContext, IIdentifiable resource)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var trackedIdentifiable = (IIdentifiable)dbContext.GetTrackedIdentifiable(resource);
            if (trackedIdentifiable == null)
            {
                dbContext.Entry(resource).State = EntityState.Unchanged;
                trackedIdentifiable = resource;
            }

            return trackedIdentifiable;
        }

        /// <summary>
        /// Searches the change tracker for an entity that matches the type and ID of <paramref name="identifiable"/>.
        /// </summary>
        public static object GetTrackedIdentifiable(this DbContext dbContext, IIdentifiable identifiable)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (identifiable == null) throw new ArgumentNullException(nameof(identifiable));

            var entityType = identifiable.GetType();
            var entityEntry = dbContext.ChangeTracker
                .Entries()
                .FirstOrDefault(entry =>
                    entry.Entity.GetType() == entityType &&
                    ((IIdentifiable) entry.Entity).StringId == identifiable.StringId);

            return entityEntry?.Entity;
        }

        /// <summary>
        /// Gets the current transaction or creates a new one.
        /// If a transaction already exists, commit, rollback and dispose
        /// will not be called. It is assumed the creator of the original
        /// transaction should be responsible for disposal.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// using(var transaction = _context.GetCurrentOrCreateTransaction())
        /// {
        ///     // perform multiple operations on the context and then save...
        ///     _context.SaveChanges();
        /// }
        /// </code>
        /// </example>
        public static async Task<IDbContextTransaction> GetCurrentOrCreateTransactionAsync(this DbContext context) 
            => await SafeTransactionProxy.GetOrCreateAsync(context.Database);
    }

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
            => (databaseFacade.CurrentTransaction != null)
                ? new SafeTransactionProxy(databaseFacade.CurrentTransaction, shouldExecute: false)
                : new SafeTransactionProxy(await databaseFacade.BeginTransactionAsync(), shouldExecute: true);

        /// <inheritdoc />
        public Guid TransactionId => _transaction.TransactionId;

        /// <inheritdoc />
        public void Commit() => Proxy(t => t.Commit());

        /// <inheritdoc />
        public Task CommitAsync(CancellationToken cancellationToken) => Proxy(t => t.CommitAsync(cancellationToken));

        /// <inheritdoc />
        public void Rollback() => Proxy(t => t.Rollback());

        /// <inheritdoc />
        public Task RollbackAsync(CancellationToken cancellationToken) => Proxy(t => t.RollbackAsync(cancellationToken));

        /// <inheritdoc />
        public void Dispose() => Proxy(t => t.Dispose());

        public ValueTask DisposeAsync()
        {
            return Proxy(t => t.DisposeAsync());
        }

        private void Proxy(Action<IDbContextTransaction> func)
        {
            if(_shouldExecute) 
                func(_transaction);
        }

        private TResult Proxy<TResult>(Func<IDbContextTransaction, TResult> func)
        {
            if(_shouldExecute) 
                return func(_transaction);
            
            return default;
        }
    }
}
