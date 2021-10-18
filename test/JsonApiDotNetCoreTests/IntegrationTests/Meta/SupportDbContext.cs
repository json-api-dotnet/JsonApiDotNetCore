using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SupportDbContext : DbContext
    {
        public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        public SupportDbContext(DbContextOptions<SupportDbContext> options)
            : base(options)
        {
        }
    }
}
