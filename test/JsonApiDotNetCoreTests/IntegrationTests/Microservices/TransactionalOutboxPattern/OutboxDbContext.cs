using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<DomainUser> Users => Set<DomainUser>();
    public DbSet<DomainGroup> Groups => Set<DomainGroup>();
    public DbSet<OutgoingMessage> OutboxMessages => Set<OutgoingMessage>();
}
