using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LinksDbContext(DbContextOptions<LinksDbContext> options) : TestableDbContext(options)
{
    public DbSet<PhotoAlbum> PhotoAlbums => Set<PhotoAlbum>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoLocation> PhotoLocations => Set<PhotoLocation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Photo>()
            .HasOne(photo => photo.Location)
            .WithOne(location => location.Photo)
            .HasForeignKey<Photo>("LocationId");

        base.OnModelCreating(builder);
    }
}
