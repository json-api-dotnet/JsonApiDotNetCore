using System;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    internal sealed class RelationshipTypeFactory
    {
        public static RelationshipTypeFactory Instance { get; } = new();

        private RelationshipTypeFactory()
        {
        }

        public Type GetForRequest(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            return NonPrimaryDocumentTypeFactory.Instance.GetForRelationshipRequest(relationship);
        }

        public Type GetForResponse(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            // @formatter:nested_ternary_style expanded

            Type relationshipDataOpenType = relationship is HasManyAttribute
                ? typeof(ToManyRelationshipInResponse<>)
                : relationship.IsNullable()
                    ? typeof(NullableToOneRelationshipInResponse<>)
                    : typeof(ToOneRelationshipInResponse<>);

            // @formatter:nested_ternary_style restore

            return relationshipDataOpenType.MakeGenericType(relationship.RightType.ClrType);
        }
    }
}
