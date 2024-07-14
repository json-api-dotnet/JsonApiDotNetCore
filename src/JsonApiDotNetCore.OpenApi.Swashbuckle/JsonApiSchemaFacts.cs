using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class JsonApiSchemaFacts
{
    private static readonly Type[] RequestBodySchemaTypes =
    [
        typeof(CreateResourceRequestDocument<>),
        typeof(UpdateResourceRequestDocument<>),
        typeof(ToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(ToManyRelationshipInRequest<>),
        typeof(OperationsRequestDocument)
    ];

    private static readonly Type[] SchemaTypesHavingNullableDataProperty =
    [
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInResponse<>),
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>)
    ];

    private static readonly Type[] RelationshipInResponseSchemaTypes =
    [
        typeof(ToOneRelationshipInResponse<>),
        typeof(ToManyRelationshipInResponse<>),
        typeof(NullableToOneRelationshipInResponse<>)
    ];

    public static bool IsRequestBodySchemaType(Type schemaType)
    {
        Type lookupType = schemaType.ConstructedToOpenType();
        return RequestBodySchemaTypes.Contains(lookupType);
    }

    public static bool HasNullableDataProperty(Type schemaType)
    {
        // Swashbuckle infers non-nullable because our Data properties are [Required].

        Type lookupType = schemaType.ConstructedToOpenType();
        return SchemaTypesHavingNullableDataProperty.Contains(lookupType);
    }

    public static bool IsRelationshipInResponseType(Type schemaType)
    {
        Type lookupType = schemaType.ConstructedToOpenType();
        return RelationshipInResponseSchemaTypes.Contains(lookupType);
    }
}
