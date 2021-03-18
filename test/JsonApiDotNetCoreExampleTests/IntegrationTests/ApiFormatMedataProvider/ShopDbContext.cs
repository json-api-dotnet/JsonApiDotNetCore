using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiFormatMedataProvider
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ShopDbContext : DbContext
    {
        public DbSet<Store> Stores { get; set; }

        public DbSet<Product> Products { get; set; }

        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            : base(options)
        {
        }
    }
}
