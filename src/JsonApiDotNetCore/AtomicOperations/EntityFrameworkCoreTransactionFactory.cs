using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <summary>
    /// Provides transaction support for atomic:operation requests using Entity Framework Core.
    /// </summary>
    public sealed class EntityFrameworkCoreTransactionFactory : IOperationsTransactionFactory
    {
        private readonly IDbContextResolver _dbContextResolver;
        private readonly IJsonApiOptions _options;

        public EntityFrameworkCoreTransactionFactory(IDbContextResolver dbContextResolver, IJsonApiOptions options)
        {
            _dbContextResolver = dbContextResolver ?? throw new ArgumentNullException(nameof(dbContextResolver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            var dbContext = _dbContextResolver.GetContext();

            var transaction = _options.TransactionIsolationLevel != null
                ? await dbContext.Database.BeginTransactionAsync(_options.TransactionIsolationLevel.Value,
                    cancellationToken)
                : await dbContext.Database.BeginTransactionAsync(cancellationToken);

            return new EntityFrameworkCoreTransaction(transaction, dbContext);
        }
    }
}
