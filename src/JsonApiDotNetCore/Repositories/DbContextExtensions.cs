using System;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.Repositories
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Determines whether or not EF is already tracking an entity of the same Type and Id
        /// and returns that entity.
        /// </summary>
        internal static TEntity GetTrackedEntity<TEntity>(this DbContext context, TEntity entity)
            where TEntity : class, IIdentifiable
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityEntry = context.ChangeTracker
                .Entries()
                .FirstOrDefault(entry =>
                    entry.Entity.GetType() == entity.GetType() &&
                    ((IIdentifiable) entry.Entity).StringId == entity.StringId);

            return (TEntity) entityEntry?.Entity;
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
}
