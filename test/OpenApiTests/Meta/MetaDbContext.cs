using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MetaDbContext(DbContextOptions<MetaDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<JigsawPuzzle> Puzzles => Set<JigsawPuzzle>();
}
