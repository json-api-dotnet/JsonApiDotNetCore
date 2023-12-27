using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.TablePerHierarchy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TablePerHierarchyDbContext(DbContextOptions<TablePerHierarchyDbContext> options) : ResourceInheritanceDbContext(options);
