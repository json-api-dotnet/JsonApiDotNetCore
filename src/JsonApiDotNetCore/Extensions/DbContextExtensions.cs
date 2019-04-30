using System;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.Extensions
{
    public static class DbContextExtensions
    {
        [Obsolete("This is no longer required since the introduction of context.Set<T>", error: false)]
        public static DbSet<T> GetDbSet<T>(this DbContext context) where T : class 
            => context.Set<T>();

        /// <summary>
        /// Given a child entity and a relationship attribute between a parent 
        /// entity to that child entity, attaches the entities on the inverse navigation
        /// property to the dbContext.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="parentToChildAttribute">Parent to child relationship attribute.</param>
        /// <param name="childEntity">Child entity.</param>
        /// <typeparam name="TParent">The 1st type parameter.</typeparam>
        public static void LoadInverseNavigation<TParent>(
            this DbContext context,
            RelationshipAttribute parentToChildAttribute,
            object childEntity) where TParent : class, IIdentifiable
        {
            var navigationMeta = context.Model
                    .FindEntityType(typeof(TParent))
                    .FindNavigation(parentToChildAttribute.InternalRelationshipName);
            var inverseNavigationMeta = navigationMeta.FindInverse();
            if (inverseNavigationMeta != null)
            {
                var inversePropertyType = inverseNavigationMeta.PropertyInfo.PropertyType;
                var inversePropertyName = inverseNavigationMeta.Name;
                var entityEntry = context.Entry(childEntity);
                if (inversePropertyType.IsGenericType ) 
                { // if generic, means we're dealing with a list
                    entityEntry.Collection(inversePropertyName).Load();
                } else
                {
                    entityEntry.Navigation(inversePropertyName).Load();
                }
            }
        }

        /// <summary>
        /// Get the DbSet when the model type is unknown until runtime
        /// </summary>
        public static IQueryable<object> Set(this DbContext context, Type t)
            => (IQueryable<object>)context
                .GetType()
                .GetMethod("Set")
                .MakeGenericMethod(t) // TODO: will caching help runtime performance?
                .Invoke(context, null);

        /// <summary>
        /// Determines whether or not EF is already tracking an entity of the same Type and Id
        /// </summary>
        public static bool EntityIsTracked(this DbContext context, IIdentifiable entity)
        {
            return GetTrackedEntity(context, entity) != null;
        }

        /// <summary>
        /// Determines whether or not EF is already tracking an entity of the same Type and Id
        /// and returns that entity.
        /// </summary>
        public static IIdentifiable GetTrackedEntity(this DbContext context, IIdentifiable entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var trackedEntries = context.ChangeTracker
                .Entries()
                .FirstOrDefault(entry =>
                    entry.Entity.GetType() == entity.GetType()
                    && ((IIdentifiable)entry.Entity).StringId == entity.StringId
                );

            return (IIdentifiable)trackedEntries?.Entity;
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
    /// <summary>
    internal struct SafeTransactionProxy : IDbContextTransaction
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
        public void Rollback() => Proxy(t => t.Rollback());
        
        /// <inheritdoc />
        public void Dispose() => Proxy(t => t.Dispose());

        private void Proxy(Action<IDbContextTransaction> func)
        {
            if(_shouldExecute) 
                func(_transaction);
        }
    }
}
