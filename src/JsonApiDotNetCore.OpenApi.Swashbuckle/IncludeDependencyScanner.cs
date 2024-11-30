using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class IncludeDependencyScanner
{
    public static IncludeDependencyScanner Instance { get; } = new();

    private IncludeDependencyScanner()
    {
    }

    /// <summary>
    /// Returns all related resource types that are reachable from the specified resource type. Does not include <paramref name="resourceType" /> itself.
    /// </summary>
    public IReadOnlySet<ResourceType> GetReachableRelatedTypes(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        HashSet<ResourceType> resourceTypesFound = [];

        IncludeResourceType(resourceType, resourceTypesFound);
        resourceTypesFound.Remove(resourceType);

        return resourceTypesFound;
    }

    private static void IncludeResourceType(ResourceType resourceType, ISet<ResourceType> resourceTypesFound)
    {
        if (resourceTypesFound.Add(resourceType))
        {
            IncludeDerivedTypes(resourceType, resourceTypesFound);
            IncludeRelatedTypes(resourceType, resourceTypesFound);
        }
    }

    private static void IncludeDerivedTypes(ResourceType resourceType, ISet<ResourceType> resourceTypesFound)
    {
        foreach (ResourceType derivedType in resourceType.DirectlyDerivedTypes)
        {
            IncludeResourceType(derivedType, resourceTypesFound);
        }
    }

    private static void IncludeRelatedTypes(ResourceType resourceType, ISet<ResourceType> resourceTypesFound)
    {
        foreach (RelationshipAttribute relationship in resourceType.Relationships)
        {
            IncludeResourceType(relationship.RightType, resourceTypesFound);
        }
    }
}
