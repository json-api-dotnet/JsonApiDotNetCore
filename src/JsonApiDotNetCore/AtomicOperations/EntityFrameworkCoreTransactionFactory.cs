using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Repositories;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Provides transaction support for atomic:operation requests using Entity Framework Core.
    /// </summary>
    public sealed class EntityFrameworkCoreTransactionFactory : IOperationsTransactionFactory
    {
        private readonly IDbContextResolver _dbContextResolver;

        public EntityFrameworkCoreTransactionFactory(IDbContextResolver dbContextResolver)
        {
            _dbContextResolver = dbContextResolver ?? throw new ArgumentNullException(nameof(dbContextResolver));
        }

        /// <inheritdoc />
        public async Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            var dbContext = _dbContextResolver.GetContext();
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            return new EntityFrameworkCoreTransaction(transaction, dbContext);
        }
    }
}
