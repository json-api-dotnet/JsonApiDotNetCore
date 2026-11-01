using FluentAssertions;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

internal static class PlaylistCompoundExtensions
{
    public static Playlist WithCompounds(this Playlist playlist)
    {
        playlist.Credits =
        [
            new PlaylistCredit
            {
                Role = "Producer",
                PersonName = "Alice"
            },
            new PlaylistCredit
            {
                Role = "Mixer",
                PersonName = "Bob"
            }
        ];

        playlist.Recording = new PlaylistRecording
        {
            Studio = "Abbey Road",
            RecordedAt = new DateOnly(2020, 5, 17),
            Engineer = "Carol"
        };

        return playlist;
    }

    public static void ShouldHaveOriginalCompoundsOf(this Playlist playlistInDatabase, Playlist original)
    {
        playlistInDatabase.Credits.Should().HaveCount(original.Credits.Count);

        for (int index = 0; index < original.Credits.Count; index++)
        {
            playlistInDatabase.Credits[index].Role.Should().Be(original.Credits[index].Role);
            playlistInDatabase.Credits[index].PersonName.Should().Be(original.Credits[index].PersonName);
        }

        playlistInDatabase.Recording.Should().NotBeNull();
        playlistInDatabase.Recording.Studio.Should().Be(original.Recording!.Studio);
        playlistInDatabase.Recording.RecordedAt.Should().Be(original.Recording.RecordedAt);
        playlistInDatabase.Recording.Engineer.Should().Be(original.Recording.Engineer);
    }
}
