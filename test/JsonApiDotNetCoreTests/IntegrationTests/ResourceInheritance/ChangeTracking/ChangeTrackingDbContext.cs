using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.ChangeTracking;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ChangeTrackingDbContext(DbContextOptions<ChangeTrackingDbContext> options)
    : ResourceInheritanceDbContext(options);
