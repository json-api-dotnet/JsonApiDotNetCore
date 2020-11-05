using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Client.Internal;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Client
{
    public sealed class ResponseDeserializerTests : DeserializerTestsSetup
    {
        private readonly Dictionary<string, string> _linkValues = new Dictionary<string, string>();
        private readonly ResponseDeserializer _deserializer;

        public ResponseDeserializerTests()
        {
            _deserializer = new ResponseDeserializer(_resourceGraph, new ResourceFactory(new ServiceContainer()));
            _linkValues.Add("self", "http://example.com/articles");
            _linkValues.Add("next", "http://example.com/articles?page[number]=2");
            _linkValues.Add("last", "http://example.com/articles?page[number]=10");
        }

        [Fact]
        public void DeserializeSingle_EmptyResponseWithMeta_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Meta = new Dictionary<string, object> { { "foo", "bar" } }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<TestResource>(body);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Meta);
            Assert.Equal("bar", result.Meta["foo"]);
        }

        [Fact]
        public void DeserializeSingle_EmptyResponseWithTopLevelLinks_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Links = new TopLevelLinks { Self = _linkValues["self"], Next = _linkValues["next"], Last = _linkValues["last"] }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<TestResource>(body);

            // Assert
            Assert.Null(result.Data);
            Assert.NotNull(result.Links);
            TopLevelLinks links = result.Links;
            Assert.Equal(_linkValues["self"], links.Self);
            Assert.Equal(_linkValues["next"], links.Next);
            Assert.Equal(_linkValues["last"], links.Last);
        }

        [Fact]
        public void DeserializeList_EmptyResponseWithTopLevelLinks_CanDeserialize()
        {
            // Arrange
            var content = new Document
            {
                Links = new TopLevelLinks { Self = _linkValues["self"], Next = _linkValues["next"], Last = _linkValues["last"] },
                Data = new List<ResourceObject>()
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeMany<TestResource>(body);

            // Assert
            Assert.Empty(result.Data);
            Assert.NotNull(result.Links);
            TopLevelLinks links = result.Links;
            Assert.Equal(_linkValues["self"], links.Self);
            Assert.Equal(_linkValues["next"], links.Next);
            Assert.Equal(_linkValues["last"], links.Last);
        }

        [Fact]
        public void DeserializeSingle_ResourceWithAttributes_CanDeserialize()
        {
            // Arrange
            var content = CreateTestResourceDocument();
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<TestResource>(body);
            var resource = result.Data;

            // Assert
            Assert.Null(result.Links);
            Assert.Null(result.Meta);
            Assert.Equal(1, resource.Id);
            Assert.Equal(content.SingleData.Attributes["stringField"], resource.StringField);
        }

        [Fact]
        public void DeserializeSingle_MultipleDependentRelationshipsWithIncluded_CanDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("multiPrincipals");
            content.SingleData.Relationships.Add("populatedToOne", CreateRelationshipData("oneToOneDependents"));
            content.SingleData.Relationships.Add("populatedToManies", CreateRelationshipData("oneToManyDependents", isToManyData: true));
            content.SingleData.Relationships.Add("emptyToOne", CreateRelationshipData());
            content.SingleData.Relationships.Add("emptyToManies", CreateRelationshipData(isToManyData: true));
            var toOneAttributeValue = "populatedToOne member content";
            var toManyAttributeValue = "populatedToManies member content";
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "oneToOneDependents",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", toOneAttributeValue } }
                },
                new ResourceObject
                {
                    Type = "oneToManyDependents",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", toManyAttributeValue } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<MultipleRelationshipsPrincipalPart>(body);
            var resource = result.Data;

            // Assert
            Assert.Equal(1, resource.Id);
            Assert.NotNull(resource.PopulatedToOne);
            Assert.Equal(toOneAttributeValue, resource.PopulatedToOne.AttributeMember);
            Assert.Equal(toManyAttributeValue, resource.PopulatedToManies.First().AttributeMember);
            Assert.NotNull(resource.PopulatedToManies);
            Assert.NotNull(resource.EmptyToManies);
            Assert.Empty(resource.EmptyToManies);
            Assert.Null(resource.EmptyToOne);
        }

        [Fact]
        public void DeserializeSingle_MultiplePrincipalRelationshipsWithIncluded_CanDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("multiDependents");
            content.SingleData.Relationships.Add("populatedToOne", CreateRelationshipData("oneToOnePrincipals"));
            content.SingleData.Relationships.Add("populatedToMany", CreateRelationshipData("oneToManyPrincipals"));
            content.SingleData.Relationships.Add("emptyToOne", CreateRelationshipData());
            content.SingleData.Relationships.Add("emptyToMany", CreateRelationshipData());
            var toOneAttributeValue = "populatedToOne member content";
            var toManyAttributeValue = "populatedToManies member content";
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "oneToOnePrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", toOneAttributeValue } }
                },
                new ResourceObject
                {
                    Type = "oneToManyPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", toManyAttributeValue } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<MultipleRelationshipsDependentPart>(body);
            var resource = result.Data;

            // Assert
            Assert.Equal(1, resource.Id);
            Assert.NotNull(resource.PopulatedToOne);
            Assert.Equal(toOneAttributeValue, resource.PopulatedToOne.AttributeMember);
            Assert.Equal(toManyAttributeValue, resource.PopulatedToMany.AttributeMember);
            Assert.NotNull(resource.PopulatedToMany);
            Assert.Null(resource.EmptyToMany);
            Assert.Null(resource.EmptyToOne);
        }

        [Fact]
        public void DeserializeSingle_NestedIncluded_CanDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("multiPrincipals");
            content.SingleData.Relationships.Add("populatedToManies", CreateRelationshipData("oneToManyDependents", isToManyData: true));
            var toManyAttributeValue = "populatedToManies member content";
            var nestedIncludeAttributeValue = "nested include member content";
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "oneToManyDependents",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", toManyAttributeValue } },
                    Relationships = new Dictionary<string, RelationshipEntry> { { "principal", CreateRelationshipData("oneToManyPrincipals") } }
                },
                new ResourceObject
                {
                    Type = "oneToManyPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", nestedIncludeAttributeValue } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<MultipleRelationshipsPrincipalPart>(body);
            var resource = result.Data;

            // Assert
            Assert.Equal(1, resource.Id);
            Assert.Null(resource.PopulatedToOne);
            Assert.Null(resource.EmptyToManies);
            Assert.Null(resource.EmptyToOne);
            Assert.NotNull(resource.PopulatedToManies);
            var includedResource = resource.PopulatedToManies.First();
            Assert.Equal(toManyAttributeValue, includedResource.AttributeMember);
            var nestedIncludedResource = includedResource.Principal;
            Assert.Equal(nestedIncludeAttributeValue, nestedIncludedResource.AttributeMember);
        }
        
        [Fact]
        public void DeserializeSingle_DeeplyNestedIncluded_CanDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("multiPrincipals");
            content.SingleData.Relationships.Add("multi", CreateRelationshipData("multiPrincipals"));
            var includedAttributeValue = "multi member content";
            var nestedIncludedAttributeValue = "nested include member content";
            var deeplyNestedIncludedAttributeValue = "deeply nested member content";
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "multiPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", includedAttributeValue } },
                    Relationships = new Dictionary<string, RelationshipEntry> { { "populatedToManies", CreateRelationshipData("oneToManyDependents", isToManyData: true) } }
                },
                new ResourceObject
                {
                    Type = "oneToManyDependents",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", nestedIncludedAttributeValue } },
                    Relationships = new Dictionary<string, RelationshipEntry> { { "principal", CreateRelationshipData("oneToManyPrincipals") } }
                },
                new ResourceObject
                {
                    Type = "oneToManyPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", deeplyNestedIncludedAttributeValue } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<MultipleRelationshipsPrincipalPart>(body);
            var resource = result.Data;

            // Assert
            Assert.Equal(1, resource.Id);
            var included = resource.Multi;
            Assert.Equal(10, included.Id);
            Assert.Equal(includedAttributeValue, included.AttributeMember);
            var nestedIncluded = included.PopulatedToManies.First();
            Assert.Equal(10, nestedIncluded.Id);
            Assert.Equal(nestedIncludedAttributeValue, nestedIncluded.AttributeMember);
            var deeplyNestedIncluded = nestedIncluded.Principal;
            Assert.Equal(10, deeplyNestedIncluded.Id);
            Assert.Equal(deeplyNestedIncludedAttributeValue, deeplyNestedIncluded.AttributeMember);
        }

        [Fact]
        public void DeserializeList_DeeplyNestedIncluded_CanDeserialize()
        {
            // Arrange
            var content = new Document { Data = new List<ResourceObject> { CreateDocumentWithRelationships("multiPrincipals").SingleData } };
            content.ManyData[0].Relationships.Add("multi", CreateRelationshipData("multiPrincipals"));
            var includedAttributeValue = "multi member content";
            var nestedIncludedAttributeValue = "nested include member content";
            var deeplyNestedIncludedAttributeValue = "deeply nested member content";
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "multiPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", includedAttributeValue } },
                    Relationships = new Dictionary<string, RelationshipEntry> { { "populatedToManies", CreateRelationshipData("oneToManyDependents", isToManyData: true) } }
                },
                new ResourceObject
                {
                    Type = "oneToManyDependents",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", nestedIncludedAttributeValue } },
                    Relationships = new Dictionary<string, RelationshipEntry> { { "principal", CreateRelationshipData("oneToManyPrincipals") } }
                },
                new ResourceObject
                {
                    Type = "oneToManyPrincipals",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { {"attributeMember", deeplyNestedIncludedAttributeValue } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeMany<MultipleRelationshipsPrincipalPart>(body);
            var resource = result.Data.First();

            // Assert
            Assert.Equal(1, resource.Id);
            var included = resource.Multi;
            Assert.Equal(10, included.Id);
            Assert.Equal(includedAttributeValue, included.AttributeMember);
            var nestedIncluded = included.PopulatedToManies.First();
            Assert.Equal(10, nestedIncluded.Id);
            Assert.Equal(nestedIncludedAttributeValue, nestedIncluded.AttributeMember);
            var deeplyNestedIncluded = nestedIncluded.Principal;
            Assert.Equal(10, deeplyNestedIncluded.Id);
            Assert.Equal(deeplyNestedIncludedAttributeValue, deeplyNestedIncluded.AttributeMember);
        }
        
        [Fact]
        public void DeserializeSingle_ResourceWithInheritanceAndInclusions_CanDeserialize()
        {
            // Arrange
            var content = CreateDocumentWithRelationships("testResourceWithAbstractRelationships");
            content.SingleData.Relationships.Add("toMany", CreateRelationshipData("firstDerivedModels", isToManyData: true, id: "10"));
            content.SingleData.Relationships["toMany"].ManyData.Add(CreateRelationshipData("secondDerivedModels", id: "11").SingleData);
            content.SingleData.Relationships.Add("toOne", CreateRelationshipData("firstDerivedModels", id: "20"));
            content.Included = new List<ResourceObject>
            {
                new ResourceObject
                {
                    Type = "firstDerivedModels",
                    Id = "10",
                    Attributes = new Dictionary<string, object> { { "firstProperty", "true" } }
                },
                new ResourceObject
                {
                    Type = "secondDerivedModels",
                    Id = "11",
                    Attributes = new Dictionary<string, object> { { "secondProperty", "false" } }
                },
                new ResourceObject
                {
                    Type = "firstDerivedModels",
                    Id = "20",
                    Attributes = new Dictionary<string, object> { { "firstProperty", "true" } }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<TestResourceWithAbstractRelationship>(body);
            var resource = result.Data;

            // Assert
            Assert.Equal(1, resource.Id);
            Assert.NotNull(resource.ToOne);
            Assert.True(resource.ToOne is FirstDerivedModel);
            Assert.True(((FirstDerivedModel)resource.ToOne).FirstProperty);
            
            Assert.NotEmpty(resource.ToMany);
            var first = resource.ToMany[0];
            Assert.True(first is FirstDerivedModel);
            Assert.True(((FirstDerivedModel)first).FirstProperty);
            
            var second = resource.ToMany[1];
            Assert.True(second is SecondDerivedModel);
            Assert.False(((SecondDerivedModel)second).SecondProperty);
        }
    }
}
