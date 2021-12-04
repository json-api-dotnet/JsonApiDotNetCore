using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PolicyDbContext : DbContext
{
    public DbSet<Policy> Policies => Set<Policy>();

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options)
    {
    }
}
