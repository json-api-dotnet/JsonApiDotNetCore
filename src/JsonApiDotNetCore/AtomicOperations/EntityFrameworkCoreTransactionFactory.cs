using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JsonApiDotNetCore.AtomicOperations;

/// <summary>
/// Provides transaction support for atomic:operation requests using Entity Framework Core.
/// </summary>
public sealed class EntityFrameworkCoreTransactionFactory : IOperationsTransactionFactory
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly IJsonApiOptions _options;

    public EntityFrameworkCoreTransactionFactory(IDbContextResolver dbContextResolver, IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(dbContextResolver);
        ArgumentNullException.ThrowIfNull(options);

        _dbContextResolver = dbContextResolver;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<IOperationsTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        DbContext dbContext = _dbContextResolver.GetContext();

        IDbContextTransaction transaction = _options.TransactionIsolationLevel != null
            ? await dbContext.Database.BeginTransactionAsync(_options.TransactionIsolationLevel.Value, cancellationToken)
            : await dbContext.Database.BeginTransactionAsync(cancellationToken);

        return new EntityFrameworkCoreTransaction(transaction, dbContext);
    }
}
