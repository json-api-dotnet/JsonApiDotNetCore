using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public sealed class PolicyDbContext : DbContext
    {
        public DbSet<Policy> Policies { get; set; }

        public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
            : base(options)
        {
        }
    }
}
