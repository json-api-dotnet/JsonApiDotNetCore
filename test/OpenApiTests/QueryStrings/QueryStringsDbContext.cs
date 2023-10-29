using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringsDbContext : TestableDbContext
{
    public DbSet<Node> Nodes => Set<Node>();

    public QueryStringsDbContext(DbContextOptions<QueryStringsDbContext> options)
        : base(options)
    {
    }
}
