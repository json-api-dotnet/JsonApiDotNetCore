using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PolicyDbContext : TestableDbContext
{
    public DbSet<Policy> Policies => Set<Policy>();

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options)
    {
    }
}
