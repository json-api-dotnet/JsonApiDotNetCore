using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerHierarchy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompoundTablePerHierarchyDbContext(DbContextOptions<CompoundTablePerHierarchyDbContext> options)
    : CompoundInheritanceDbContext(options);
