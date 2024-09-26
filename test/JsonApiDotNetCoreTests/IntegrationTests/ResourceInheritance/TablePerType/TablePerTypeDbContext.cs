using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerType;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TablePerTypeDbContext(DbContextOptions<TablePerTypeDbContext> options)
    : ResourceInheritanceDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Vehicle>()
            .UseTptMappingStrategy();

        builder.Entity<Wheel>()
            .UseTptMappingStrategy();

        builder.Entity<Engine>()
            .UseTptMappingStrategy();

        builder.Entity<GenericProperty>()
            .UseTptMappingStrategy();

        base.OnModelCreating(builder);
    }
}
