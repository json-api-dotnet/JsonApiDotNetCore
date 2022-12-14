using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BlobDbContext : TestableDbContext
{
    public DbSet<ImageContainer> ImageContainers => Set<ImageContainer>();

    public BlobDbContext(DbContextOptions<BlobDbContext> options)
        : base(options)
    {
    }
}
