using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LoggingDbContext : DbContext
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }
}
