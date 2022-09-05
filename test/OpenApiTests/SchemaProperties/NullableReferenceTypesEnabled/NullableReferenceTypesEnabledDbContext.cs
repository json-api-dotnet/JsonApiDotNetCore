using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NullableReferenceTypesEnabledDbContext : DbContext
{
    public DbSet<Cow> Cow => Set<Cow>();

    public NullableReferenceTypesEnabledDbContext(DbContextOptions<NullableReferenceTypesEnabledDbContext> options)
        : base(options)
    {
    }
}
