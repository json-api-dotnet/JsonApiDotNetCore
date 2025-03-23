using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class JsonApiSchemaFacts
{
    private static readonly Type[] RequestBodySchemaTypes =
    [
        typeof(CreateRequestDocument<>),
        typeof(UpdateRequestDocument<>),
        typeof(ToOneInRequest<>),
        typeof(NullableToOneInRequest<>),
        typeof(ToManyInRequest<>),
        typeof(OperationsRequestDocument)
    ];

    private static readonly Type[] SchemaTypesHavingNullableDataProperty =
    [
        typeof(NullableToOneInRequest<>),
        typeof(NullableToOneInResponse<>),
        typeof(NullableSecondaryResponseDocument<>),
        typeof(NullableIdentifierResponseDocument<>)
    ];

    private static readonly Type[] RelationshipInResponseSchemaTypes =
    [
        typeof(ToOneInResponse<>),
        typeof(ToManyInResponse<>),
        typeof(NullableToOneInResponse<>)
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
