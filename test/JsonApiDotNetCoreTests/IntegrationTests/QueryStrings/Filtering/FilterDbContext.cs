using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FilterDbContext : DbContext
    {
        public DbSet<FilterableResource> FilterableResources { get; set; }

        public FilterDbContext(DbContextOptions<FilterDbContext> options)
            : base(options)
        {
        }
    }
}
