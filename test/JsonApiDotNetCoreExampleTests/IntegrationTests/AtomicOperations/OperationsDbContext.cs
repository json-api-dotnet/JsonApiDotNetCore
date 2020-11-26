using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class OperationsDbContext : DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Performer> Performers { get; set; }
        public DbSet<RecordCompany> RecordCompanies { get; set; }

        public OperationsDbContext(DbContextOptions<OperationsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlaylistMusicTrack>()
                .HasKey(playlistMusicTrack => new {playlistMusicTrack.PlaylistId, playlistMusicTrack.MusicTrackId});
        }
    }
}
