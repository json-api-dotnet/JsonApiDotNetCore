using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class FireForgetDbContext(DbContextOptions<FireForgetDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<DomainUser> Users => Set<DomainUser>();
    public DbSet<DomainGroup> Groups => Set<DomainGroup>();
}
