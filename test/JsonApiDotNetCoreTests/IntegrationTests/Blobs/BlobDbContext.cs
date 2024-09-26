using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class BlobDbContext(DbContextOptions<BlobDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<ImageContainer> ImageContainers => Set<ImageContainer>();
}
