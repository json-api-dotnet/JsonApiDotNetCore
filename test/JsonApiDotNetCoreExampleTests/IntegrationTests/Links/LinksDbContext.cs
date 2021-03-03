using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class LinksDbContext : DbContext
    {
        public DbSet<PhotoAlbum> PhotoAlbums { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoLocation> PhotoLocations { get; set; }

        public LinksDbContext(DbContextOptions<LinksDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Photo>()
                .HasOne(photo => photo.Location)
                .WithOne(location => location.Photo)
                .HasForeignKey<Photo>("PhotoLocationKey");
        }
    }
}
