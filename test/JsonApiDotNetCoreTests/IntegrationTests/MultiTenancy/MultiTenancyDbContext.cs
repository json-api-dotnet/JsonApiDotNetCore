using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MultiTenancyDbContext(DbContextOptions<MultiTenancyDbContext> options, ITenantProvider tenantProvider) : TestableDbContext(options)
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;

    public DbSet<WebShop> WebShops => Set<WebShop>();
    public DbSet<WebProduct> WebProducts => Set<WebProduct>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<WebShop>()
            .HasMany(webShop => webShop.Products)
            .WithOne(webProduct => webProduct.Shop);

        builder.Entity<WebShop>()
            .HasQueryFilter(webShop => webShop.TenantId == _tenantProvider.TenantId);

        builder.Entity<WebProduct>()
            .HasQueryFilter(webProduct => webProduct.Shop.TenantId == _tenantProvider.TenantId);

        base.OnModelCreating(builder);
    }
}
