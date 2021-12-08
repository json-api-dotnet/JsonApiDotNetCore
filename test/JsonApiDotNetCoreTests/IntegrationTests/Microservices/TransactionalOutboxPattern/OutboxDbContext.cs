using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OutboxDbContext : DbContext
{
    public DbSet<DomainUser> Users => Set<DomainUser>();
    public DbSet<DomainGroup> Groups => Set<DomainGroup>();
    public DbSet<OutgoingMessage> OutboxMessages => Set<OutgoingMessage>();

    public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
        : base(options)
    {
    }
}
