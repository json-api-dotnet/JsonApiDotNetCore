using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TablePerHierarchyDbContext : ResourceInheritanceDbContext
{
    public TablePerHierarchyDbContext(DbContextOptions<TablePerHierarchyDbContext> options)
        : base(options)
    {
    }
}
