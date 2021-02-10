using System;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.QueryStrings
{
    public sealed class MusicTrackReleaseDefinition : JsonApiResourceDefinition<MusicTrack, Guid>
    {
        private readonly ISystemClock _systemClock;

        public MusicTrackReleaseDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
            : base(resourceGraph)
        {
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
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
            if (bool.Parse(parameterValue))
            {
                source = source.Where(musicTrack =>
                    musicTrack.ReleasedAt < _systemClock.UtcNow &&
                    musicTrack.ReleasedAt > _systemClock.UtcNow.AddMonths(-3));
            }

            return source;
        }
    }
}
