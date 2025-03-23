using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

/// <summary>
/// Represents a generic component schema type, whose first type parameter implements <see cref="IIdentifiable" />. Examples:
/// <see cref="CreateRequestDocument{TResource}" />, <see cref="UpdateToManyRelationshipOperation{TResource}" />,
/// <see cref="NullableToOneInResponse{TResource}" />, <see cref="AttributesInResponse{TResource}" />.
/// </summary>
internal sealed class ResourceSchemaType
{
    public Type SchemaOpenType { get; }
    public ResourceType ResourceType { get; }

    private ResourceSchemaType(Type schemaOpenType, ResourceType resourceType)
    {
        SchemaOpenType = schemaOpenType;
        ResourceType = resourceType;
    }

    public static ResourceSchemaType Create(Type schemaConstructedType, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(schemaConstructedType);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        Type schemaOpenType = schemaConstructedType.GetGenericTypeDefinition();
        Type resourceClrType = schemaConstructedType.GenericTypeArguments[0];
        ResourceType resourceType = resourceGraph.GetResourceType(resourceClrType);

        return new ResourceSchemaType(schemaOpenType, resourceType);
    }

    public override string ToString()
    {
        return $"{SchemaOpenType.Name} for {ResourceType.ClrType.Name}";
    }
}
