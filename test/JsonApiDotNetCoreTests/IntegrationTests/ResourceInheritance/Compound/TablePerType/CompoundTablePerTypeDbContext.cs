using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerType;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompoundTablePerTypeDbContext(DbContextOptions<CompoundTablePerTypeDbContext> options)
    : CompoundInheritanceDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Listing>()
            .UseTptMappingStrategy();

        base.OnModelCreating(builder);
    }
}
