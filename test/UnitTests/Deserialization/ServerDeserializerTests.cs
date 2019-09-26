using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Deserialization
{
    public class ServerDeserializerTests : DeserializerTestsSetup
    {
        private readonly ServerDeserializer _deserializer;
        private readonly Mock<IUpdatedFieldsManager> _fieldsManagerMock = new Mock<IUpdatedFieldsManager>();
        public ServerDeserializerTests() : base()
        {
            _deserializer = new ServerDeserializer(_resourceGraph, _defaultSettings, _fieldsManagerMock.Object);
        }

        [Fact]
        public void DeserializeAttributes_VariousUpdatedMembers_RegistersUpdatedFields()
        {
            // arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            Document content = CreateTestResourceDocument();
            var body = JsonConvert.SerializeObject(content);

            // act
            _deserializer.Deserialize(body);

            // assert
            Assert.Equal(5, attributesToUpdate.Count);
            Assert.Empty(relationshipsToUpdate);
        }

        [Fact]
        public void DeserializeAttributes_UpdatedImmutableMember_ThrowsInvalidOperationException()
        {
            // arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "immutable", "some string" },
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // act, assert
            Assert.Throws<InvalidOperationException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_MultipleDependentRelationships_RegistersUpdatedRelationships()
        {
            // arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = CreateDocumentWithRelationships("multi-principals");
            content.Data.Relationships.Add("populated-to-one", CreateRelationshipData("one-to-one-dependents"));
            content.Data.Relationships.Add("empty-to-one", CreateRelationshipData());
            content.Data.Relationships.Add("populated-to-manies", CreateRelationshipData("one-to-many-dependents", isToManyData: true));
            content.Data.Relationships.Add("empty-to-manies", CreateRelationshipData(isToManyData: true));
            var body = JsonConvert.SerializeObject(content);

            // act
            _deserializer.Deserialize(body);

            // assert
            Assert.Equal(4, relationshipsToUpdate.Count);
            Assert.Empty(attributesToUpdate);
        }

        [Fact]
        public void DeserializeRelationships_MultiplePrincipalRelationships_RegistersUpdatedRelationships()
        {
            // arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = CreateDocumentWithRelationships("multi-dependents");
            content.Data.Relationships.Add("populated-to-one", CreateRelationshipData("one-to-one-principals"));
            content.Data.Relationships.Add("empty-to-one", CreateRelationshipData());
            content.Data.Relationships.Add("populated-to-many", CreateRelationshipData("one-to-many-principals"));
            content.Data.Relationships.Add("empty-to-many", CreateRelationshipData());
            var body = JsonConvert.SerializeObject(content);

            // act
            _deserializer.Deserialize(body);

            // assert
            Assert.Equal(4, relationshipsToUpdate.Count);
            Assert.Empty(attributesToUpdate);
        }

        private void SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate)
        {
            attributesToUpdate = new List<AttrAttribute>();
            relationshipsToUpdate = new List<RelationshipAttribute>();
            _fieldsManagerMock.Setup(m => m.AttributesToUpdate).Returns(attributesToUpdate);
            _fieldsManagerMock.Setup(m => m.RelationshipsToUpdate).Returns(relationshipsToUpdate);
        }
    }
}
