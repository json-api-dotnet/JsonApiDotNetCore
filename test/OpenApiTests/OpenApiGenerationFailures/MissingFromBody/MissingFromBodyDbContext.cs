using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MissingFromBodyDbContext(DbContextOptions<MissingFromBodyDbContext> options) : TestableDbContext(options)
{
    public DbSet<RecycleBin> RecycleBins => Set<RecycleBin>();
}
