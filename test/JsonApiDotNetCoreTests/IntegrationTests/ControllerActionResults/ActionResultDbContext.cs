using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ActionResultDbContext : TestableDbContext
{
    public DbSet<Toothbrush> Toothbrushes => Set<Toothbrush>();

    public ActionResultDbContext(DbContextOptions<ActionResultDbContext> options)
        : base(options)
    {
    }
}
