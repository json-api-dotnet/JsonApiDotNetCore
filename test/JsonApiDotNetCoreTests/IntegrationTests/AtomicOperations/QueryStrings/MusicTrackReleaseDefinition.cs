using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.QueryStrings;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MusicTrackReleaseDefinition : JsonApiResourceDefinition<MusicTrack, Guid>
{
    private readonly TimeProvider _timeProvider;

    public MusicTrackReleaseDefinition(IResourceGraph resourceGraph, TimeProvider timeProvider)
        : base(resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
    }

    public override QueryStringParameterHandlers<MusicTrack> OnRegisterQueryableHandlersForQueryStringParameters()
    {
        return new QueryStringParameterHandlers<MusicTrack>
        {
            ["isRecentlyReleased"] = FilterOnRecentlyReleased
        };
    }

    private IQueryable<MusicTrack> FilterOnRecentlyReleased(IQueryable<MusicTrack> source, StringValues parameterValue)
    {
        IQueryable<MusicTrack> tracks = source;

        if (bool.Parse(parameterValue.ToString()))
        {
            DateTimeOffset utcNow = _timeProvider.GetUtcNow();
            tracks = tracks.Where(musicTrack => musicTrack.ReleasedAt < utcNow && musicTrack.ReleasedAt > utcNow.AddMonths(-3));
        }

        return tracks;
    }
}
