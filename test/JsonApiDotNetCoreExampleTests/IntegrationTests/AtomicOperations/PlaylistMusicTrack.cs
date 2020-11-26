namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class PlaylistMusicTrack
    {
        public long PlaylistId { get; set; }
        public Playlist Playlist { get; set; }

        public long MusicTrackId { get; set; }
        public MusicTrack MusicTrack { get; set; }
    }
}
