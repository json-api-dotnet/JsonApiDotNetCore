using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OperationsDbContext : DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<PlaylistMusicTrack> PlaylistMusicTracks { get; set; }
        public DbSet<Lyric> Lyrics { get; set; }
        public DbSet<TextLanguage> TextLanguages { get; set; }
        public DbSet<Performer> Performers { get; set; }
        public DbSet<RecordCompany> RecordCompanies { get; set; }

        public OperationsDbContext(DbContextOptions<OperationsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlaylistMusicTrack>()
                .HasKey(playlistMusicTrack => new
                {
                    playlistMusicTrack.PlaylistId,
                    playlistMusicTrack.MusicTrackId
                });

            builder.Entity<MusicTrack>()
                .HasOne(musicTrack => musicTrack.Lyric)
                .WithOne(lyric => lyric.Track)
                .HasForeignKey<MusicTrack>();
        }
    }
}
