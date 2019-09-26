using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;

namespace UnitTests.Deserialization
{
    public class DeserializerTestsSetup
    {
        protected readonly IResourceGraph _resourceGraph;
        protected readonly JsonApiSerializerSettings _defaultSettings = new JsonApiSerializerSettings();

        public DeserializerTestsSetup()
        {
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            resourceGraphBuilder.AddResource<TestResourceWithList>("test-resource-with-list");
            // one to one relationships
            resourceGraphBuilder.AddResource<OneToOnePrincipal>("one-to-one-principals");
            resourceGraphBuilder.AddResource<OneToOneDependent>("one-to-one-dependents");
            resourceGraphBuilder.AddResource<OneToOneRequiredDependent>("one-to-one-required-dependents");
            // one to many relationships
            resourceGraphBuilder.AddResource<OneToManyPrincipal>("one-to-many-principals");
            resourceGraphBuilder.AddResource<OneToManyDependent>("one-to-many-dependents");
            resourceGraphBuilder.AddResource<OneToManyRequiredDependent>("one-to-many-required-dependents");
            // collective relationships
            resourceGraphBuilder.AddResource<MultipleRelationshipsPrincipalPart>("multi-principals");
            resourceGraphBuilder.AddResource<MultipleRelationshipsDependentPart>("multi-dependents");
            _resourceGraph = resourceGraphBuilder.Build();
        }

        protected Document CreateDocumentWithRelationships(string mainType, string relationshipMemberName, string relatedType = null, bool isToManyData = false)
        {
            var content = CreateDocumentWithRelationships(mainType);
            content.Data.Relationships.Add(relationshipMemberName, CreateRelationshipData(relatedType, isToManyData));
            return content;
        }

        protected Document CreateDocumentWithRelationships(string mainType)
        {
            return new Document
            {
                Data = new ResourceObject
                {
                    Id = "1",
                    Type = mainType,
                    Relationships = new Dictionary<string, RelationshipData> { }
                }
            };
        }

        protected RelationshipData CreateRelationshipData(string relatedType = null, bool isToManyData = false)
        {
            var data = new RelationshipData();
            var rio = relatedType == null ? null : new ResourceIdentifierObject { Id = "10", Type = relatedType };

            if (isToManyData)
            {
                data.ExposedData = new List<ResourceIdentifierObject>();
                if (relatedType != null) ((List<ResourceIdentifierObject>)data.ExposedData).Add(rio);
            } else
            {
                data.ExposedData = rio;
            }
            return data;
        }

        protected Document CreateTestResourceDocument()
        {
            return new Document
            {
                Data = new ResourceObject
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "string-field", "some string" },
                        { "int-field", 1 },
                        { "nullable-int-field", null },
                        { "guid-field", "1a68be43-cc84-4924-a421-7f4d614b7781" },
                        { "date-time-field", "9/11/2019 11:41:40 AM" }
                    }
                }
            };
        }

        protected class TestResource : Identifiable
        {
            [Attr] public string StringField { get; set; }
            [Attr] public DateTime DateTimeField { get; set; }
            [Attr] public DateTime? NullableDateTimeField { get; set; }
            [Attr] public int IntField { get; set; }
            [Attr] public int? NullableIntField { get; set; }
            [Attr] public Guid GuidField { get; set; }
            [Attr] public ComplexType ComplexField { get; set; }
            [Attr(isImmutable: true)] public string Immutable { get; set; }
        }

        protected class TestResourceWithList : Identifiable
        {
            [Attr] public List<ComplexType> ComplexFields { get; set; }
        }

        protected class ComplexType
        {
            public string CompoundName { get; set; }
        }

        protected class OneToOnePrincipal : IdentifiableWithAttribute
        {
            [HasOne] public OneToOneDependent Dependent { get; set; }
        }

        protected class OneToOneDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal Principal { get; set; }
            public int? PrincipalId { get; set; }
        }

        protected class OneToOneRequiredDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal Principal { get; set; }
            public int PrincipalId { get; set; }
        }

        protected class OneToManyDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToManyPrincipal Principal { get; set; }
            public int? PrincipalId { get; set; }
        }

        protected class OneToManyRequiredDependent : IdentifiableWithAttribute
        {
            [HasOne] public OneToManyPrincipal Principal { get; set; }
            public int PrincipalId { get; set; }
        }

        protected class OneToManyPrincipal : IdentifiableWithAttribute
        {
            [HasMany] public List<OneToManyDependent> Dependents { get; set; }
        }

        protected class IdentifiableWithAttribute : Identifiable
        {
            [Attr] public string AttributeMember { get; set; }
        }

        protected class MultipleRelationshipsPrincipalPart : IdentifiableWithAttribute
        {
            [HasOne] public OneToOneDependent PopulatedToOne { get; set; }
            [HasOne] public OneToOneDependent EmptyToOne { get; set; }
            [HasMany] public List<OneToManyDependent> PopulatedToManies { get; set; }
            [HasMany] public List<OneToManyDependent> EmptyToManies { get; set; }
            [HasOne] public MultipleRelationshipsPrincipalPart Multi { get; set; }
        }

        protected class MultipleRelationshipsDependentPart : IdentifiableWithAttribute
        {
            [HasOne] public OneToOnePrincipal PopulatedToOne { get; set; }
            public int PopulatedToOneId { get; set; }
            [HasOne] public OneToOnePrincipal EmptyToOne { get; set; }
            public int? EmptyToOneId { get; set; }
            [HasOne] public OneToManyPrincipal PopulatedToMany { get; set; }
            public int PopulatedToManyId { get; set; }
            [HasOne] public OneToManyPrincipal EmptyToMany { get; set; }
            public int? EmptyToManyId { get; set; }
        }
    }
}
