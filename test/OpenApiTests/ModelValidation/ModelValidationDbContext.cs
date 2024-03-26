using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ModelValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelValidationDbContext(DbContextOptions<ModelValidationDbContext> options) : TestableDbContext(options)
{
    public DbSet<Fingerprint> Fingerprints => Set<Fingerprint>();
}
