using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesDisabledDbContext : TestableDbContext
{
    public DbSet<Chicken> Chicken => Set<Chicken>();

    public NullableReferenceTypesDisabledDbContext(DbContextOptions<NullableReferenceTypesDisabledDbContext> options)
        : base(options)
    {
    }
}
