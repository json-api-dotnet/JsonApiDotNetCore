#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.TransactionalOutboxPattern
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OutboxDbContext : DbContext
    {
        public DbSet<DomainUser> Users { get; set; }
        public DbSet<DomainGroup> Groups { get; set; }
        public DbSet<OutgoingMessage> OutboxMessages { get; set; }

        public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
            : base(options)
        {
        }
    }
}
