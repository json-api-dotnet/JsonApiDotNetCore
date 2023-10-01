using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories;

/// <inheritdoc cref="IDbContextResolver" />
[PublicAPI]
public sealed class DbContextResolver<TDbContext> : IDbContextResolver
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public DbContextResolver(TDbContext dbContext)
    {
        ArgumentGuard.NotNull(dbContext);

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
