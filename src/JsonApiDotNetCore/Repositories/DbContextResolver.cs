using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Repositories;

/// <inheritdoc cref="IDbContextResolver" />
/// <typeparam name="TDbContext">
/// The type of the <see cref="DbContext" /> to resolve.
/// </typeparam>
[PublicAPI]
public sealed class DbContextResolver<TDbContext> : IDbContextResolver
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IReferencingForeignKeysCache _foreignKeysCache;

    public DbContextResolver(TDbContext dbContext)
        : this(dbContext, new ReferencingForeignKeysCache())
    {
    }

    public DbContextResolver(TDbContext dbContext, IReferencingForeignKeysCache foreignKeysCache)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(foreignKeysCache);

        _dbContext = dbContext;
        _foreignKeysCache = foreignKeysCache;
    }

    public DbContext GetContext()
    {
        return _dbContext;
    }

    public TDbContext GetTypedContext()
    {
        return _dbContext;
    }

    /// <inheritdoc />
    public IReadOnlyList<IReadOnlyForeignKey> GetForeignKeysRequiringClientSetNullOnDelete(Type entityClrType)
    {
        ArgumentNullException.ThrowIfNull(entityClrType);

        return _foreignKeysCache.GetForeignKeysRequiringClientSetNullOnDelete(_dbContext.Model, entityClrType);
    }
}
