using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FireForgetDbContext : DbContext
{
    public DbSet<DomainUser> Users => Set<DomainUser>();
    public DbSet<DomainGroup> Groups => Set<DomainGroup>();

    public FireForgetDbContext(DbContextOptions<FireForgetDbContext> options)
        : base(options)
    {
    }
}
