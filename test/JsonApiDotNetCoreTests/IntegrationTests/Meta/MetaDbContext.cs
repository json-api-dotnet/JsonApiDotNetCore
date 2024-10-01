using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MetaDbContext(DbContextOptions<MetaDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
}
