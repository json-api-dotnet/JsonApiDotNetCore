using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class HostingDbContext : DbContext
    {
        public DbSet<ArtGallery> ArtGalleries { get; set; }
        public DbSet<Painting> Paintings { get; set; }

        public HostingDbContext(DbContextOptions<HostingDbContext> options)
            : base(options)
        {
        }
    }
}
