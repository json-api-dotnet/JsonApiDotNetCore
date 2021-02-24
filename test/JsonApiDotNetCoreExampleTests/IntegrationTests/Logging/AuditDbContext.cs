using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AuditDbContext : DbContext
    {
        public DbSet<AuditEntry> AuditEntries { get; set; }

        public AuditDbContext(DbContextOptions<AuditDbContext> options)
            : base(options)
        {
        }
    }
}
