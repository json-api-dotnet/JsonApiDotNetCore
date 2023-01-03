using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects;

internal sealed class RelationshipTypeFactory
{
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly NonPrimaryDocumentTypeFactory _nonPrimaryDocumentTypeFactory;

    public RelationshipTypeFactory(ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        _nonPrimaryDocumentTypeFactory = new NonPrimaryDocumentTypeFactory(resourceFieldValidationMetadataProvider);
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
