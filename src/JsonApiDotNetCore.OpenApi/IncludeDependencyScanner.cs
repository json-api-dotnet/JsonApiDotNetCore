using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class IncludeDependencyScanner
{
    /// <summary>
    /// Returns all related resource types that are reachable from the specified resource type. May include <paramref name="resourceType" /> itself.
    /// </summary>
    public IReadOnlySet<ResourceType> GetReachableRelatedTypes(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        var resourceTypesFound = new HashSet<ResourceType>();
        AddTypesFromRelationships(resourceType.Relationships, resourceTypesFound);
        return resourceTypesFound;
    }

    private static void AddTypesFromRelationships(IEnumerable<RelationshipAttribute> relationships, ISet<ResourceType> resourceTypesFound)
    {
        foreach (RelationshipAttribute relationship in relationships)
        {
            ResourceType resourceType = relationship.RightType;

            if (resourceTypesFound.Add(resourceType))
            {
                AddTypesFromRelationships(resourceType.Relationships, resourceTypesFound);
            }
        }
    }
}
