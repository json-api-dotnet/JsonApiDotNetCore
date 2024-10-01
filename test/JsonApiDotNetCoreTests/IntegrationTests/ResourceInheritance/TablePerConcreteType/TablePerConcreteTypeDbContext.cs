using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerConcreteType;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TablePerConcreteTypeDbContext(DbContextOptions<TablePerConcreteTypeDbContext> options)
    : ResourceInheritanceDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Vehicle>()
            .UseTpcMappingStrategy();

        builder.Entity<Wheel>()
            .UseTpcMappingStrategy();

        builder.Entity<Engine>()
            .UseTpcMappingStrategy();

        builder.Entity<GenericProperty>()
            .UseTpcMappingStrategy();

        base.OnModelCreating(builder);
    }
}
