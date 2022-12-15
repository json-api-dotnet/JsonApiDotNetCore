using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FireForgetDbContext : TestableDbContext
{
    public DbSet<DomainUser> Users => Set<DomainUser>();
    public DbSet<DomainGroup> Groups => Set<DomainGroup>();

    public FireForgetDbContext(DbContextOptions<FireForgetDbContext> options)
        : base(options)
    {
    }
}
