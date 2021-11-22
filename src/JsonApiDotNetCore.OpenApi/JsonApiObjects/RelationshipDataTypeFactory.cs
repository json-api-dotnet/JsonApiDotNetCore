using System;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    internal sealed class RelationshipDataTypeFactory
    {
        public static RelationshipDataTypeFactory Instance { get; } = new();

        private RelationshipDataTypeFactory()
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
                ? typeof(ToManyRelationshipResponseData<>)
                : relationship.IsNullable()
                    ? typeof(NullableToOneRelationshipResponseData<>)
                    : typeof(ToOneRelationshipResponseData<>);

            // @formatter:nested_ternary_style restore

            return relationshipDataOpenType.MakeGenericType(relationship.RightType.ClrType);
        }
    }
}
