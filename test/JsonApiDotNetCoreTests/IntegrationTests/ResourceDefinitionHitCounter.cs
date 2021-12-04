using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests;

/// <summary>
/// Used to keep track of invocations on <see cref="IResourceDefinition{TResource,TId}" /> callback methods.
/// </summary>
public sealed class ResourceDefinitionHitCounter
{
    internal IList<(Type, ResourceDefinitionExtensibilityPoints)> HitExtensibilityPoints { get; } =
        new List<(Type, ResourceDefinitionExtensibilityPoints)>();

    internal void TrackInvocation<TResource>(ResourceDefinitionExtensibilityPoints extensibilityPoint)
        where TResource : IIdentifiable
    {
        HitExtensibilityPoints.Add((typeof(TResource), extensibilityPoint));
    }

    internal void Reset()
    {
        HitExtensibilityPoints.Clear();
    }
}
