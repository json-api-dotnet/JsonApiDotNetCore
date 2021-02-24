using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CustomRouteDbContext : DbContext
    {
        public DbSet<Town> Towns { get; set; }
        public DbSet<Civilian> Civilians { get; set; }

        public CustomRouteDbContext(DbContextOptions<CustomRouteDbContext> options)
            : base(options)
        {
        }
    }
}
