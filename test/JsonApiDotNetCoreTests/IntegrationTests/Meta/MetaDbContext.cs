using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MetaDbContext : TestableDbContext
{
    public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    public MetaDbContext(DbContextOptions<MetaDbContext> options)
        : base(options)
    {
    }
}
