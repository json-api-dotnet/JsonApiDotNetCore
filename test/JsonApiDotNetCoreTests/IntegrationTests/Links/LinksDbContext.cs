using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class LinksDbContext : DbContext
    {
        public DbSet<PhotoAlbum> PhotoAlbums => Set<PhotoAlbum>();
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<PhotoLocation> PhotoLocations => Set<PhotoLocation>();

        public LinksDbContext(DbContextOptions<LinksDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Photo>()
                .HasOne(photo => photo.Location)
                .WithOne(location => location!.Photo)
                .HasForeignKey<Photo>("LocationId");
        }
    }
}
