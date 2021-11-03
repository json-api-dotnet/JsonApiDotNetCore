using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class MetaDbContext : DbContext
    {
        public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        public MetaDbContext(DbContextOptions<MetaDbContext> options)
            : base(options)
        {
        }
    }
}
