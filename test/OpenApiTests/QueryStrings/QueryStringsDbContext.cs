using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringsDbContext(DbContextOptions<QueryStringsDbContext> options) : TestableDbContext(options)
{
    public DbSet<Node> Nodes => Set<Node>();
}
