using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class NonPrimaryDocumentTypeFactory
{
    private static readonly DocumentOpenTypes SecondaryResponseDocumentOpenTypes = new(typeof(CollectionResponseDocument<>),
        typeof(NullableSecondaryResponseDocument<>), typeof(SecondaryResponseDocument<>));

    private static readonly DocumentOpenTypes RelationshipRequestDocumentOpenTypes = new(typeof(ToManyInRequest<>),
        typeof(NullableToOneInRequest<>), typeof(ToOneInRequest<>));

    private static readonly DocumentOpenTypes RelationshipResponseDocumentOpenTypes = new(typeof(IdentifierCollectionResponseDocument<>),
        typeof(NullableIdentifierResponseDocument<>), typeof(IdentifierResponseDocument<>));

    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;

    public NonPrimaryDocumentTypeFactory(ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);

        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
    }

    public Type GetForSecondaryResponse(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return Get(relationship, SecondaryResponseDocumentOpenTypes);
    }

    public Type GetForRelationshipRequest(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return Get(relationship, RelationshipRequestDocumentOpenTypes);
    }

    public Type GetForRelationshipResponse(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return Get(relationship, RelationshipResponseDocumentOpenTypes);
    }

    private Type Get(RelationshipAttribute relationship, DocumentOpenTypes types)
    {
        // @formatter:nested_ternary_style expanded

        Type documentOpenType = relationship is HasManyAttribute
            ? types.ManyDataOpenType
            : _resourceFieldValidationMetadataProvider.IsNullable(relationship)
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
            ArgumentNullException.ThrowIfNull(manyDataOpenType);
            ArgumentNullException.ThrowIfNull(nullableSingleDataOpenType);
            ArgumentNullException.ThrowIfNull(singleDataOpenType);

            ManyDataOpenType = manyDataOpenType;
            NullableSingleDataOpenType = nullableSingleDataOpenType;
            SingleDataOpenType = singleDataOpenType;
        }
    }
}
