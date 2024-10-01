using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PolicyDbContext(DbContextOptions<PolicyDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Policy> Policies => Set<Policy>();
}
