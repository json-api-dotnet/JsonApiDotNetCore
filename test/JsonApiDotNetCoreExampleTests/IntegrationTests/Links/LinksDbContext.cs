using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class LinksDbContext : DbContext
    {
        public DbSet<PhotoAlbum> PhotoAlbums { get; set; }
        public DbSet<Photo> Photos { get; set; }

        public LinksDbContext(DbContextOptions<LinksDbContext> options)
            : base(options)
        {
        }
    }
}
