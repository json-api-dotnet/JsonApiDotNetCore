using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CustomRouteDbContext : DbContext
    {
        public DbSet<Town> Towns => Set<Town>();
        public DbSet<Civilian> Civilians => Set<Civilian>();

        public CustomRouteDbContext(DbContextOptions<CustomRouteDbContext> options)
            : base(options)
        {
        }
    }
}
