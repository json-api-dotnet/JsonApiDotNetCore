using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;

internal sealed class RelationshipTypeFactory
{
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;

    public RelationshipTypeFactory(NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(nonPrimaryDocumentTypeFactory);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);

        _nonPrimaryDocumentTypeFactory = nonPrimaryDocumentTypeFactory;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
    }

    public Type GetForRequest(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        return _nonPrimaryDocumentTypeFactory.GetForRelationshipRequest(relationship);
    }

    public Type GetForResponse(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        // @formatter:nested_ternary_style expanded

        Type relationshipDataOpenType = relationship is HasManyAttribute
            ? typeof(ToManyRelationshipInResponse<>)
            : _resourceFieldValidationMetadataProvider.IsNullable(relationship)
                ? typeof(NullableToOneRelationshipInResponse<>)
                : typeof(ToOneRelationshipInResponse<>);

        // @formatter:nested_ternary_style restore

        return relationshipDataOpenType.MakeGenericType(relationship.RightType.ClrType);
    }
}
