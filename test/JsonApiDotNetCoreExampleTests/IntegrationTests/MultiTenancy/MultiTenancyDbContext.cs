using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class MultiTenancyDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public DbSet<WebShop> WebShops { get; set; }
        public DbSet<WebProduct> WebProducts { get; set; }

        public MultiTenancyDbContext(DbContextOptions<MultiTenancyDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<WebShop>()
                .HasMany(webShop => webShop.Products)
                .WithOne(webProduct => webProduct.Shop)
                .IsRequired();

            builder.Entity<WebShop>()
                .HasQueryFilter(webShop => webShop.TenantId == _tenantProvider.TenantId);

            builder.Entity<WebProduct>()
                .HasQueryFilter(webProduct => webProduct.Shop.TenantId == _tenantProvider.TenantId);
        }
    }
}
