using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringDbContext(DbContextOptions<QueryStringDbContext> options) : TestableDbContext(options)
{
    public DbSet<Node> Nodes => Set<Node>();
}
