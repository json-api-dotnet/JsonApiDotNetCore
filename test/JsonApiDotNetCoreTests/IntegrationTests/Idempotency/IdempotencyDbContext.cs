using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class IdempotencyDbContext : DbContext
{
    public DbSet<Tree> Trees => Set<Tree>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Leaf> Leaves => Set<Leaf>();

    public DbSet<RequestCacheItem> RequestCache => Set<RequestCacheItem>();

    public IdempotencyDbContext(DbContextOptions<IdempotencyDbContext> options)
        : base(options)
    {
    }
}
