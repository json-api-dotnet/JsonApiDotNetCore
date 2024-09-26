using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ActionResultDbContext(DbContextOptions<ActionResultDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Toothbrush> Toothbrushes => Set<Toothbrush>();
}
