using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class KnownDbContext : DbContext
    {
        public DbSet<KnownResource> KnownResources => Set<KnownResource>();

        public KnownDbContext(DbContextOptions<KnownDbContext> options)
            : base(options)
        {
        }
    }
}
