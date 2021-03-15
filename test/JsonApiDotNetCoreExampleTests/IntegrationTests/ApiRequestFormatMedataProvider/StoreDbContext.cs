using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class StoreDbContext : DbContext
    {
        public DbSet<Store> Stores { get; set; }

        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options)
        {
        }
    }
}
