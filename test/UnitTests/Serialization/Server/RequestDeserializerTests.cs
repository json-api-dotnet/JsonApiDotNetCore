using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using Moq;
using Newtonsoft.Json;
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
            // Arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            Document content = CreateTestResourceDocument();
            var body = JsonConvert.SerializeObject(content);

            // Act
            _deserializer.Deserialize(body);

            // Assert
            Assert.Equal(5, attributesToUpdate.Count);
            Assert.Empty(relationshipsToUpdate);
        }

        [Fact]
        public void DeserializeAttributes_UpdatedImmutableMember_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = new Document
            {
                Data = new ResourceObject
                {
                    Type = "testResource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "immutable", "some string" },
                    }
                }
            };
            var body = JsonConvert.SerializeObject(content);

            // Act, assert
            Assert.Throws<InvalidOperationException>(() => _deserializer.Deserialize(body));
        }

        [Fact]
        public void DeserializeRelationships_MultipleDependentRelationships_RegistersUpdatedRelationships()
        {
            // Arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = CreateDocumentWithRelationships("multiPrincipals");
            content.SingleData.Relationships.Add("populatedToOne", CreateRelationshipData("oneToOneDependents"));
            content.SingleData.Relationships.Add("emptyToOne", CreateRelationshipData());
            content.SingleData.Relationships.Add("populatedToManies", CreateRelationshipData("oneToManyDependents", isToManyData: true));
            content.SingleData.Relationships.Add("emptyToManies", CreateRelationshipData(isToManyData: true));
            var body = JsonConvert.SerializeObject(content);

            // Act
            _deserializer.Deserialize(body);

            // Assert
            Assert.Equal(4, relationshipsToUpdate.Count);
            Assert.Empty(attributesToUpdate);
        }

        [Fact]
        public void DeserializeRelationships_MultiplePrincipalRelationships_RegistersUpdatedRelationships()
        {
            // Arrange
            SetupFieldsManager(out List<AttrAttribute> attributesToUpdate, out List<RelationshipAttribute> relationshipsToUpdate);
            var content = CreateDocumentWithRelationships("multiDependents");
            content.SingleData.Relationships.Add("populatedToOne", CreateRelationshipData("oneToOnePrincipals"));
            content.SingleData.Relationships.Add("emptyToOne", CreateRelationshipData());
            content.SingleData.Relationships.Add("populatedToMany", CreateRelationshipData("oneToManyPrincipals"));
            content.SingleData.Relationships.Add("emptyToMany", CreateRelationshipData());
            var body = JsonConvert.SerializeObject(content);

            // Act
            _deserializer.Deserialize(body);

            // Assert
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
