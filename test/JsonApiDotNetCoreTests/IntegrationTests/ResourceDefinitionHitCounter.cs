using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests
{
    /// <summary>
    /// Used to keep track of invocations on <see cref="IResourceDefinition{TResource,TId}" /> callback methods.
    /// </summary>
    public sealed class ResourceDefinitionHitCounter
    {
        internal IList<(Type, ResourceDefinitionExtensibilityPoint)> HitExtensibilityPoints { get; } = new List<(Type, ResourceDefinitionExtensibilityPoint)>();

        internal void TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoint extensibilityPoint)
            where TResource : IIdentifiable
        {
            HitExtensibilityPoints.Add((typeof(TResource), extensibilityPoint));
        }

        internal void Reset()
        {
            HitExtensibilityPoints.Clear();
        }
    }
}
