using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerConcreteType;

// Known EF Core 10 limitation: inserting a House or Apartment fails with ArgumentNullException in ModificationCommand.HandleJson,
// because the abstract base type Residence declares JSON-mapped complex properties. Inserting a Parking works.
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompoundTablePerConcreteTypeDbContext(DbContextOptions<CompoundTablePerConcreteTypeDbContext> options)
    : CompoundInheritanceDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Listing>()
            .UseTpcMappingStrategy();

        base.OnModelCreating(builder);
    }
}
