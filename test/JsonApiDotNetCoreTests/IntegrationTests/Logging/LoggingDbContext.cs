using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LoggingDbContext : TestableDbContext
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }
}
