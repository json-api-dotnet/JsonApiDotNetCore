using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.ModelStateValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelStateValidationDbContext(DbContextOptions<ModelStateValidationDbContext> options) : TestableDbContext(options)
{
    public DbSet<SocialMediaAccount> SocialMediaAccounts => Set<SocialMediaAccount>();
}
