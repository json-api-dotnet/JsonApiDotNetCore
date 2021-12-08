using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BlobDbContext : DbContext
{
    public DbSet<ImageContainer> ImageContainers => Set<ImageContainer>();

    public BlobDbContext(DbContextOptions<BlobDbContext> options)
        : base(options)
    {
    }
}
