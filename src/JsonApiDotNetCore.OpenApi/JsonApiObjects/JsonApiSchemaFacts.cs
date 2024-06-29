using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects;

internal static class JsonApiSchemaFacts
{
    private static readonly Type[] RequestSchemaTypes =
    [
        typeof(CreateResourceRequestDocument<>),
        typeof(UpdateResourceRequestDocument<>),
        typeof(ToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(ToManyRelationshipInRequest<>)
    ];

    private static readonly Type[] ResponseSchemaTypes =
    [
        typeof(ResourceCollectionResponseDocument<>),
        typeof(PrimaryResourceResponseDocument<>),
        typeof(SecondaryResourceResponseDocument<>),
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(ResourceIdentifierCollectionResponseDocument<>),
        typeof(ResourceIdentifierResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>),
        typeof(ErrorResponseDocument)
    ];

    private static readonly Type[] SchemaTypesHavingNullableDataProperty =
    [
        typeof(NullableSecondaryResourceResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>),
        typeof(NullableToOneRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInResponse<>)
    ];

    private static readonly Type[] RelationshipInResponseSchemaTypes =
    [
        typeof(ToOneRelationshipInResponse<>),
        typeof(ToManyRelationshipInResponse<>),
        typeof(NullableToOneRelationshipInResponse<>)
    ];

    public static bool RequiresCustomSchemaGenerator(Type schemaType)
    {
        // Subset of the entry types Swashbuckle calls us for, which must be handled by our custom schema generators.

        Type lookupType = schemaType.ConstructedToOpenType();
        return RequestSchemaTypes.Contains(lookupType) || ResponseSchemaTypes.Contains(lookupType);
    }

    public static bool IsRequestSchemaType(Type schemaType)
    {
        Type lookupType = schemaType.ConstructedToOpenType();
        return RequestSchemaTypes.Contains(lookupType);
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
