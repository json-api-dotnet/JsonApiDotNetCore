using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using NoDbConsoleQueryExample.Interop;
using NoDbConsoleQueryExample.Models;
using NoDbConsoleQueryExample.Repositories;

namespace NoDbConsoleQueryExample;

public sealed class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        await ExecuteInScopeAsync(scope.ServiceProvider, stoppingToken);
    }

    private async Task ExecuteInScopeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // JsonApiMiddleware depends on ASP.NET, so we must setup IJsonApiRequest ourselves.
        SetupJsonApiRequest<Track>(serviceProvider);

        // ASP.NET action filters do not execute, so we must invoke the query string reader ourselves.
        const string queryString = "?filter=greaterThan(lengthInSeconds,'225')&include=artist,genre";
        ParseQueryString(serviceProvider, queryString);

        // Gather data and make it accessible to repository.
        var dataSourceProvider = serviceProvider.GetRequiredService<IDataSourceProvider<Track, long>>();
        IEnumerable<Track> allTracks = CreateSampleData();
        dataSourceProvider.Set(allTracks);

        // Controllers depend on ASP.NET, so instead we invoke from the resource service layer.
        var trackService = serviceProvider.GetRequiredService<IResourceService<Track, long>>();
        IReadOnlyCollection<Track> tracks = await trackService.GetAsync(cancellationToken);

        PrintTracks(tracks);
    }

    private static void SetupJsonApiRequest<TResource>(IServiceProvider serviceProvider)
        where TResource : class, IIdentifiable
    {
        var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();

        var request = (JsonApiRequest)serviceProvider.GetRequiredService<IJsonApiRequest>();
        request.Kind = EndpointKind.Primary;
        request.PrimaryResourceType = resourceGraph.GetResourceType<TResource>();
        request.IsCollection = true;
        request.IsReadOnly = true;
    }

    private static void ParseQueryString(IServiceProvider serviceProvider, string queryString)
    {
        var queryStringAccessor = (InjectableRequestQueryStringAccessor)serviceProvider.GetRequiredService<IRequestQueryStringAccessor>();
        queryStringAccessor.SetFromText(queryString);

        var queryStringReader = serviceProvider.GetRequiredService<IQueryStringReader>();
        queryStringReader.ReadAll(null);
    }

    private static IEnumerable<Track> CreateSampleData()
    {
        var artist = new Artist
        {
            Name = "AC/DC"
        };

        var genre = new Genre
        {
            Name = "Hard rock"
        };

        var tracks = new List<Track>
        {
            new()
            {
                FileName = "01-fly-on-the-wall.mp3",
                DisplayName = "Fly On The Wall",
                LengthInSeconds = 224,
                Genre = genre,
                Artist = artist
            },
            new()
            {
                FileName = "02-first-blood.mp3",
                DisplayName = "First Blood",
                LengthInSeconds = 226,
                Genre = genre,
                Artist = artist
            },
            new()
            {
                FileName = "03-sink-the-pink.mp3",
                DisplayName = "Sink The Pink",
                LengthInSeconds = 255,
                Genre = genre,
                Artist = artist
            }
        };

        // Set bi-directional object references manually (normally EF Core handles this)
        foreach (Track track in tracks)
        {
            track.Artist.Tracks.Add(track);
            track.Genre.Tracks.Add(track);
        }

        return tracks;
    }

    private void PrintTracks(IReadOnlyCollection<Track> tracks)
    {
        Console.WriteLine($"Found {tracks.Count} matching track(s)");

        foreach (Track track in tracks)
        {
            Console.WriteLine($"  {track.DisplayName} ({TimeSpan.FromSeconds(track.LengthInSeconds)}) - {track.Artist.Name} ({track.Genre.Name})");
        }
    }
}
