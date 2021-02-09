using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    public sealed class AuditDbContext : DbContext
    {
        public DbSet<AuditEntry> AuditEntries { get; set; }

        public AuditDbContext(DbContextOptions<AuditDbContext> options)
            : base(options)
        {
        }
    }
}
