using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Serialization.Client;
using Newtonsoft.Json;
using Xunit;
using UnitTests.TestModels;

namespace UnitTests.Serialization.Client
{
    public sealed class ResponseDeserializerTests : DeserializerTestsSetup
    {
        private readonly Dictionary<string, string> _linkValues = new Dictionary<string, string>();
        private readonly ResponseDeserializer _deserializer;

        public ResponseDeserializerTests()
        {
            _deserializer = new ResponseDeserializer(_resourceGraph, new ServiceContainer());
            _linkValues.Add("self", "http://example.com/articles");
            _linkValues.Add("next", "http://example.com/articles?page[offset]=2");
            _linkValues.Add("last", "http://example.com/articles?page[offset]=10");
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
            var result = _deserializer.DeserializeList<TestResource>(body);

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
            var entity = result.Data;

            // Assert
            Assert.Null(result.Links);
            Assert.Null(result.Meta);
            Assert.Equal(1, entity.Id);
            Assert.Equal(content.SingleData.Attributes["stringField"], entity.StringField);
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
            var entity = result.Data;

            // Assert
            Assert.Equal(1, entity.Id);
            Assert.NotNull(entity.PopulatedToOne);
            Assert.Equal(toOneAttributeValue, entity.PopulatedToOne.AttributeMember);
            Assert.Equal(toManyAttributeValue, entity.PopulatedToManies.First().AttributeMember);
            Assert.NotNull(entity.PopulatedToManies);
            Assert.NotNull(entity.EmptyToManies);
            Assert.Empty(entity.EmptyToManies);
            Assert.Null(entity.EmptyToOne);
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
            var entity = result.Data;

            // Assert
            Assert.Equal(1, entity.Id);
            Assert.NotNull(entity.PopulatedToOne);
            Assert.Equal(toOneAttributeValue, entity.PopulatedToOne.AttributeMember);
            Assert.Equal(toManyAttributeValue, entity.PopulatedToMany.AttributeMember);
            Assert.NotNull(entity.PopulatedToMany);
            Assert.Null(entity.EmptyToMany);
            Assert.Null(entity.EmptyToOne);
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
            var entity = result.Data;

            // Assert
            Assert.Equal(1, entity.Id);
            Assert.Null(entity.PopulatedToOne);
            Assert.Null(entity.EmptyToManies);
            Assert.Null(entity.EmptyToOne);
            Assert.NotNull(entity.PopulatedToManies);
            var includedEntity = entity.PopulatedToManies.First();
            Assert.Equal(toManyAttributeValue, includedEntity.AttributeMember);
            var nestedIncludedEntity = includedEntity.Principal;
            Assert.Equal(nestedIncludeAttributeValue, nestedIncludedEntity.AttributeMember);
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
                },
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeSingle<MultipleRelationshipsPrincipalPart>(body);
            var entity = result.Data;

            // Assert
            Assert.Equal(1, entity.Id);
            var included = entity.Multi;
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
                },
            };
            var body = JsonConvert.SerializeObject(content);

            // Act
            var result = _deserializer.DeserializeList<MultipleRelationshipsPrincipalPart>(body);
            var entity = result.Data.First();

            // Assert
            Assert.Equal(1, entity.Id);
            var included = entity.Multi;
            Assert.Equal(10, included.Id);
            Assert.Equal(includedAttributeValue, included.AttributeMember);
            var nestedIncluded = included.PopulatedToManies.First();
            Assert.Equal(10, nestedIncluded.Id);
            Assert.Equal(nestedIncludedAttributeValue, nestedIncluded.AttributeMember);
            var deeplyNestedIncluded = nestedIncluded.Principal;
            Assert.Equal(10, deeplyNestedIncluded.Id);
            Assert.Equal(deeplyNestedIncludedAttributeValue, deeplyNestedIncluded.AttributeMember);
        }
    }
}
