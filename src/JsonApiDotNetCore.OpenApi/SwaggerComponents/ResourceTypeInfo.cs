using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceTypeInfo
{
    public Type ResourceDataOpenType { get; }
    public ResourceType ResourceType { get; }

    private ResourceTypeInfo(Type resourceDataOpenType, ResourceType resourceType)
    {
        ResourceDataOpenType = resourceDataOpenType;
        ResourceType = resourceType;
    }

    public static ResourceTypeInfo Create(Type resourceDataConstructedType, IResourceGraph resourceGraph)
    {
        ArgumentGuard.NotNull(resourceDataConstructedType);
        ArgumentGuard.NotNull(resourceGraph);

        Type resourceDataOpenType = resourceDataConstructedType.GetGenericTypeDefinition();
        Type resourceClrType = resourceDataConstructedType.GenericTypeArguments[0];
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

        return new ResourceTypeInfo(resourceDataOpenType, resourceType);
    }

    public override string ToString()
    {
        return $"{ResourceDataOpenType.Name} for {ResourceType.ClrType.Name}";
    }
}
