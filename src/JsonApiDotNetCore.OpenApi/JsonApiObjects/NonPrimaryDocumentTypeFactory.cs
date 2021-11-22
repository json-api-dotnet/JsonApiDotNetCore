using System;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    internal sealed class NonPrimaryDocumentTypeFactory
    {
        private static readonly DocumentOpenTypeOptions SecondaryResponseDocumentTypeOptions = new(typeof(ResourceCollectionResponseDocument<>),
            typeof(NullableSecondaryResourceResponseDocument<>), typeof(SecondaryResourceResponseDocument<>));

        private static readonly DocumentOpenTypeOptions RelationshipRequestDocumentTypeOptions = new(typeof(ToManyRelationshipRequestData<>),
            typeof(NullableToOneRelationshipRequestData<>), typeof(ToOneRelationshipRequestData<>));

        private static readonly DocumentOpenTypeOptions RelationshipResponseDocumentTypeOptions = new(typeof(ResourceIdentifierCollectionResponseDocument<>),
            typeof(NullableResourceIdentifierResponseDocument<>), typeof(ResourceIdentifierResponseDocument<>));

        public static NonPrimaryDocumentTypeFactory Instance { get; } = new();

        private NonPrimaryDocumentTypeFactory()
        {
        }

        public Type GetForSecondaryResponse(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            return Get(relationship, SecondaryResponseDocumentTypeOptions);
        }

        public Type GetForRelationshipRequest(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            return Get(relationship, RelationshipRequestDocumentTypeOptions);
        }

        public Type GetForRelationshipResponse(RelationshipAttribute relationship)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));

            return Get(relationship, RelationshipResponseDocumentTypeOptions);
        }

        private static Type Get(RelationshipAttribute relationship, DocumentOpenTypeOptions typeOptions)
        {
            // @formatter:nested_ternary_style expanded

            Type documentOpenType = relationship is HasManyAttribute
                ? typeOptions.ManyData
                : relationship.IsNullable()
                    ? typeOptions.NullableSingleData
                    : typeOptions.SingleData;

            // @formatter:nested_ternary_style restore

            return documentOpenType.MakeGenericType(relationship.RightType.ClrType);
        }

        private sealed class DocumentOpenTypeOptions
        {
            public Type ManyData { get; }
            public Type NullableSingleData { get; }
            public Type SingleData { get; }

            public DocumentOpenTypeOptions(Type manyDataOpenType, Type nullableSingleDataOpenType, Type singleDataOpenType)
            {
                ArgumentGuard.NotNull(manyDataOpenType, nameof(manyDataOpenType));
                ArgumentGuard.NotNull(nullableSingleDataOpenType, nameof(nullableSingleDataOpenType));
                ArgumentGuard.NotNull(singleDataOpenType, nameof(singleDataOpenType));

                ManyData = manyDataOpenType;
                NullableSingleData = nullableSingleDataOpenType;
                SingleData = singleDataOpenType;
            }
        }
    }
}
