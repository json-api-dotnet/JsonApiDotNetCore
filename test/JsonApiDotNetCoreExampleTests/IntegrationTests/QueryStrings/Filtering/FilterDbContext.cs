using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterDbContext : DbContext
    {
        public DbSet<FilterableResource> FilterableResources { get; set; }

        public FilterDbContext(DbContextOptions<FilterDbContext> options) : base(options)
        {
        }
    }
}
