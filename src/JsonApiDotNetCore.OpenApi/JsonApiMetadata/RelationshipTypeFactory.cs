using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata;

internal sealed class RelationshipTypeFactory
{
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;

    public RelationshipTypeFactory(NonPrimaryDocumentTypeFactory nonPrimaryDocumentTypeFactory,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(nonPrimaryDocumentTypeFactory);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _nonPrimaryDocumentTypeFactory = nonPrimaryDocumentTypeFactory;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
    }

    public Type GetForRequest(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        return _nonPrimaryDocumentTypeFactory.GetForRelationshipRequest(relationship);
    }

    public Type GetForResponse(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

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
