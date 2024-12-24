using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

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

    public DbContextResolver(TDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public DbContext GetContext()
    {
        return _dbContext;
    }

    public TDbContext GetTypedContext()
    {
        return _dbContext;
    }
}
