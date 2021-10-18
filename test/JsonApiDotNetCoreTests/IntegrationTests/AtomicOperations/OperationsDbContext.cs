using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OperationsDbContext : DbContext
    {
        public DbSet<Playlist> Playlists => Set<Playlist>();
        public DbSet<MusicTrack> MusicTracks => Set<MusicTrack>();
        public DbSet<Lyric> Lyrics => Set<Lyric>();
        public DbSet<TextLanguage> TextLanguages => Set<TextLanguage>();
        public DbSet<Performer> Performers => Set<Performer>();
        public DbSet<RecordCompany> RecordCompanies => Set<RecordCompany>();

        public OperationsDbContext(DbContextOptions<OperationsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MusicTrack>()
                .HasOne(musicTrack => musicTrack.Lyric)
                .WithOne(lyric => lyric.Track)
                .HasForeignKey<MusicTrack>("LyricId");

            builder.Entity<MusicTrack>()
                .HasMany(musicTrack => musicTrack.OccursIn)
                .WithMany(playlist => playlist.Tracks);
        }
    }
}
