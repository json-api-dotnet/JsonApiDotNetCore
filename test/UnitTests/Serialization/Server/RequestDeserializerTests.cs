using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using Moq;
using Newtonsoft.Json;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;
using Xunit;


namespace UnitTests.Serialization.Server
{
    public class RequestDeserializerTests : DeserializerTestsSetup
    {
        private readonly RequestDeserializer _deserializer;
        private readonly Mock<ITargetedFields> _fieldsManagerMock = new Mock<ITargetedFields>();
        public RequestDeserializerTests() : base()
        {
            _deserializer = new RequestDeserializer(_resourceGraph, _fieldsManagerMock.Object);
        }

        [Fact]
        public void DeserializeAttributes_VariousUpdatedMembers_RegistersTargetedFields()
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
            content.SingleData.Relationships.Add("populated-to-one", CreateRelationshipData("one-to-one-dependents"));
            content.SingleData.Relationships.Add("empty-to-one", CreateRelationshipData());
            content.SingleData.Relationships.Add("populated-to-manies", CreateRelationshipData("one-to-many-dependents", isToManyData: true));
            content.SingleData.Relationships.Add("empty-to-manies", CreateRelationshipData(isToManyData: true));
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
            content.SingleData.Relationships.Add("populated-to-one", CreateRelationshipData("one-to-one-principals"));
            content.SingleData.Relationships.Add("empty-to-one", CreateRelationshipData());
            content.SingleData.Relationships.Add("populated-to-many", CreateRelationshipData("one-to-many-principals"));
            content.SingleData.Relationships.Add("empty-to-many", CreateRelationshipData());
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
            _fieldsManagerMock.Setup(m => m.Attributes).Returns(attributesToUpdate);
            _fieldsManagerMock.Setup(m => m.Relationships).Returns(relationshipsToUpdate);
        }
    }
}
