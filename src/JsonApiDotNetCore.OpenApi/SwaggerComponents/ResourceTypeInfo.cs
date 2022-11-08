using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceTypeInfo
{
    public Type ResourceObjectOpenType { get; }
    public ResourceType ResourceType { get; }

    private ResourceTypeInfo(Type resourceObjectOpenType, ResourceType resourceType)
    {
        ResourceObjectOpenType = resourceObjectOpenType;
        ResourceType = resourceType;
    }

    public static ResourceTypeInfo Create(Type resourceObjectType, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceObjectType);
        ArgumentGuard.NotNull(resourceGraph);

        Type resourceObjectOpenType = resourceObjectType.GetGenericTypeDefinition();
        Type resourceClrType = resourceObjectType.GenericTypeArguments[0];
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

        return new ResourceTypeInfo(resourceObjectOpenType, resourceType);
    }
}
