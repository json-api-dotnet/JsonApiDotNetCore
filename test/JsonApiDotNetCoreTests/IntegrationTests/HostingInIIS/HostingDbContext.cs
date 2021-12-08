using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class HostingDbContext : DbContext
{
    public DbSet<ArtGallery> ArtGalleries => Set<ArtGallery>();
    public DbSet<Painting> Paintings => Set<Painting>();

    public HostingDbContext(DbContextOptions<HostingDbContext> options)
        : base(options)
    {
    }
}
