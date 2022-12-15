using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesEnabledDbContext : TestableDbContext
{
    public DbSet<Cow> Cow => Set<Cow>();

    public NullableReferenceTypesEnabledDbContext(DbContextOptions<NullableReferenceTypesEnabledDbContext> options)
        : base(options)
    {
    }
}
