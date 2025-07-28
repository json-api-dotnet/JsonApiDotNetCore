using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class JsonApiSchemaFacts
{
    private static readonly Type[] RequestDocumentSchemaOpenTypes =
    [
        typeof(CreateRequestDocument<>),
        typeof(UpdateRequestDocument<>),
        typeof(ToOneInRequest<>),
        typeof(NullableToOneInRequest<>),
        typeof(ToManyInRequest<>)
    ];

    private static readonly Type[] ResponseDocumentSchemaOpenTypes =
    [
        typeof(CollectionResponseDocument<>),
        typeof(PrimaryResponseDocument<>),
        typeof(SecondaryResponseDocument<>),
        typeof(NullableSecondaryResponseDocument<>),
        typeof(IdentifierResponseDocument<>),
        typeof(NullableIdentifierResponseDocument<>),
        typeof(IdentifierCollectionResponseDocument<>)
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

    public static bool IsRequestDocumentSchemaType(Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaType);

        Type lookupType = schemaType.ConstructedToOpenType();
        return RequestDocumentSchemaOpenTypes.Contains(lookupType);
    }

    public static bool IsResponseDocumentSchemaType(Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaType);

        Type lookupType = schemaType.ConstructedToOpenType();
        return ResponseDocumentSchemaOpenTypes.Contains(lookupType);
    }

    public static bool HasNullableDataProperty(Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaType);

        // Swashbuckle infers non-nullable because our Data properties are [Required].

        Type lookupType = schemaType.ConstructedToOpenType();
        return SchemaTypesHavingNullableDataProperty.Contains(lookupType);
    }

    public static bool IsRelationshipInResponseType(Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaType);

        Type lookupType = schemaType.ConstructedToOpenType();
        return RelationshipInResponseSchemaTypes.Contains(lookupType);
    }
}
