using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesDisabledDbContext : DbContext
{
    public DbSet<Chicken> Chicken => Set<Chicken>();

    public NullableReferenceTypesDisabledDbContext(DbContextOptions<NullableReferenceTypesDisabledDbContext> options)
        : base(options)
    {
    }
}
