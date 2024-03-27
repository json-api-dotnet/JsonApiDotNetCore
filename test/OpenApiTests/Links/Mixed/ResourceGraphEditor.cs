using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Links.Mixed;

/// <summary>
/// Enables changing declared links on resource types and relationships, so the model does not need to be duplicated for testing.
/// </summary>
internal sealed class ResourceGraphEditor
{
    private readonly Dictionary<Type, ResourceType> _resourceTypesByClrTypeName;

    public ResourceGraphEditor(ResourceGraphBuilder builder)
    {
        IResourceGraph resourceGraph = builder.Build();
        _resourceTypesByClrTypeName = resourceGraph.GetResourceTypes().ToDictionary(resourceType => resourceType.ClrType, resourceType => resourceType);
    }

    public void ChangeLinksInResourceType(Type resourceClrType, LinkTypes? topLevelLinks, LinkTypes? resourceLinks, LinkTypes? relationshipLinks)
    {
        ResourceType resourceType = _resourceTypesByClrTypeName[resourceClrType];

        _resourceTypesByClrTypeName[resourceClrType] = new ResourceType(resourceType.PublicName, resourceType.ClientIdGeneration, resourceType.ClrType,
            resourceType.IdentityClrType, resourceType.Attributes, resourceType.Relationships, resourceType.EagerLoads,
            topLevelLinks ?? resourceType.TopLevelLinks, resourceLinks ?? resourceType.ResourceLinks, relationshipLinks ?? resourceType.RelationshipLinks);
    }

    public void ChangeLinkInRelationship(Type resourceClrType, string relationshipPropertyName, LinkTypes links)
    {
        ResourceType resourceType = _resourceTypesByClrTypeName[resourceClrType];

        RelationshipAttribute relationship = resourceType.GetRelationshipByPropertyName(relationshipPropertyName);
        relationship.Links = links;
    }

    public IResourceGraph GetResourceGraph()
    {
        HashSet<ResourceType> resourceTypeSet = _resourceTypesByClrTypeName.Values.ToHashSet();
        return new ResourceGraph(resourceTypeSet);
    }
}
