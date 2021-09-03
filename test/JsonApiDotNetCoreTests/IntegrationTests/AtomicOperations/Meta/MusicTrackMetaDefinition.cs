using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Meta
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class MusicTrackMetaDefinition : JsonApiResourceDefinition<MusicTrack, Guid>
    {
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public MusicTrackMetaDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            _hitCounter = hitCounter;
        }

        public override IDictionary<string, object> GetMeta(MusicTrack resource)
        {
            _hitCounter.TrackInvocation<MusicTrack>(ResourceDefinitionHitCounter.ExtensibilityPoint.GetMeta);

            return new Dictionary<string, object>
            {
                ["Copyright"] = $"(C) {resource.ReleasedAt.Year}. All rights reserved."
            };
        }
    }
}
