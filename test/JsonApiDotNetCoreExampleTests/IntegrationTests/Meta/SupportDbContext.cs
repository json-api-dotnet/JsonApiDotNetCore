using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class SupportDbContext : DbContext
    {
        public DbSet<ProductFamily> ProductFamilies { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }

        public SupportDbContext(DbContextOptions<SupportDbContext> options)
            : base(options)
        {
        }
    }
}
