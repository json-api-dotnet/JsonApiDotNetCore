using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects;

internal sealed class NonPrimaryDocumentTypeFactory
{
    private static readonly DocumentOpenTypes SecondaryResponseDocumentOpenTypes = new(typeof(ResourceCollectionResponseDocument<>),
        typeof(NullableSecondaryResourceResponseDocument<>), typeof(SecondaryResourceResponseDocument<>));

    private static readonly DocumentOpenTypes RelationshipRequestDocumentOpenTypes = new(typeof(ToManyRelationshipInRequest<>),
        typeof(NullableToOneRelationshipInRequest<>), typeof(ToOneRelationshipInRequest<>));

    private static readonly DocumentOpenTypes RelationshipResponseDocumentOpenTypes = new(typeof(ResourceIdentifierCollectionResponseDocument<>),
        typeof(NullableResourceIdentifierResponseDocument<>), typeof(ResourceIdentifierResponseDocument<>));

    public static NonPrimaryDocumentTypeFactory Instance { get; } = new();

    private NonPrimaryDocumentTypeFactory()
    {
    }

    public Type GetForSecondaryResponse(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        return Get(relationship, SecondaryResponseDocumentOpenTypes);
    }

    public Type GetForRelationshipRequest(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        return Get(relationship, RelationshipRequestDocumentOpenTypes);
    }

    public Type GetForRelationshipResponse(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        return Get(relationship, RelationshipResponseDocumentOpenTypes);
    }

    private static Type Get(RelationshipAttribute relationship, DocumentOpenTypes types)
    {
        // @formatter:nested_ternary_style expanded

        Type documentOpenType = relationship is HasManyAttribute
            ? types.ManyDataOpenType
            : relationship.IsNullable()
                ? types.NullableSingleDataOpenType
                : types.SingleDataOpenType;

        // @formatter:nested_ternary_style restore

        return documentOpenType.MakeGenericType(relationship.RightType.ClrType);
    }

    private sealed class DocumentOpenTypes
    {
        public Type ManyDataOpenType { get; }
        public Type NullableSingleDataOpenType { get; }
        public Type SingleDataOpenType { get; }

        public DocumentOpenTypes(Type manyDataOpenType, Type nullableSingleDataOpenType, Type singleDataOpenType)
        {
            ArgumentGuard.NotNull(manyDataOpenType);
            ArgumentGuard.NotNull(nullableSingleDataOpenType);
            ArgumentGuard.NotNull(singleDataOpenType);

            ManyDataOpenType = manyDataOpenType;
            NullableSingleDataOpenType = nullableSingleDataOpenType;
            SingleDataOpenType = singleDataOpenType;
        }
    }
}
