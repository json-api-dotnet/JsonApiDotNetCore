using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class IdCompactionDbContext(DbContextOptions<IdCompactionDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Grant> Grants => Set<Grant>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<CompactGuid>()
            .HaveConversion<CompactGuidConverter>();
    }
}
