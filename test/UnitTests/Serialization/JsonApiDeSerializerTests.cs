using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace UnitTests.Serialization
{
    public class JsonApiDeSerializerTests
    {
        [Fact]
        public void Can_Deserialize_Complex_Types()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resource");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complex-member", new { compoundName = "testName" } }
                    }
                }
            };

            // act
            var result = deserializer.Deserialize<TestResource>(JsonConvert.SerializeObject(content));

            // assert
            Assert.NotNull(result.ComplexMember);
            Assert.Equal("testName", result.ComplexMember.CompoundName);
        }

        [Fact]
        public void Can_Deserialize_Complex_List_Types()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResourceWithList>("test-resource");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "complex-members", new [] { new { compoundName = "testName" } } }
                    }
                }
            };

            // act
            var result = deserializer.Deserialize<TestResourceWithList>(JsonConvert.SerializeObject(content));

            // assert
            Assert.NotNull(result.ComplexMembers);
            Assert.NotEmpty(result.ComplexMembers);
            Assert.Equal("testName", result.ComplexMembers[0].CompoundName);
        }

        [Fact]
        public void Can_Deserialize_Complex_Types_With_Dasherized_Attrs()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resource");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new DasherizedResolver(); // <--
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        {
                            "complex-member", new Dictionary<string, string> { { "compound-name", "testName" } }
                        }
                    }
                }
            };

            // act
            var result = deserializer.Deserialize<TestResource>(JsonConvert.SerializeObject(content));

            // assert
            Assert.NotNull(result.ComplexMember);
            Assert.Equal("testName", result.ComplexMember.CompoundName);
        }

        [Fact]
        public void Immutable_Attrs_Are_Not_Included_In_AttributesToUpdate()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resource");
            var contextGraph = contextGraphBuilder.Build();

            var attributesToUpdate = new Dictionary<AttrAttribute, object>();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(attributesToUpdate);

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new DasherizedResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        {
                            "complex-member", new Dictionary<string, string> { { "compound-name", "testName" } }
                        },
                        { "immutable", "value" }
                    }
                }
            };

            var contentString = JsonConvert.SerializeObject(content);

            // act
            var result = deserializer.Deserialize<TestResource>(contentString);

            // assert
            Assert.NotNull(result.ComplexMember);
            Assert.Equal(1, attributesToUpdate.Count);

            foreach (var attr in attributesToUpdate)
                Assert.False(attr.Key.IsImmutable);
        }

        [Fact]
        public void Can_Deserialize_Independent_Side_Of_One_To_One_Relationship()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<Independent>("independents");
            contextGraphBuilder.AddResource<Dependent>("dependents");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();
            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "independents",
                    Id = "1",
                    Attributes = new Dictionary<string, object> { { "property", property } }
                }
            };

            var contentString = JsonConvert.SerializeObject(content);

            // act
            var result = deserializer.Deserialize<Independent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(property, result.Property);
        }

        [Fact]
        public void Can_Deserialize_Independent_Side_Of_One_To_One_Relationship_With_Relationship_Body()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<Independent>("independents");
            contextGraphBuilder.AddResource<Dependent>("dependents");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();
            var content = new Document
            {
                Data = new DocumentData
                {
                    Type = "independents",
                    Id = "1",
                    Attributes = new Dictionary<string, object> { { "property", property } },
                    // a common case for this is deserialization in unit tests
                    Relationships = new Dictionary<string, RelationshipData> { { "dependent", new RelationshipData { } } }
                }
            };

            var contentString = JsonConvert.SerializeObject(content);

            // act
            var result = deserializer.Deserialize<Independent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(property, result.Property);
        }

        [Fact]
        public void Sets_The_DocumentMeta_Property_In_JsonApiContext()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<Independent>("independents");
            contextGraphBuilder.AddResource<Dependent>("dependents");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);


            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();

            var content = new Document
            {
                Meta = new Dictionary<string, object>() { { "foo", "bar" } },
                Data = new DocumentData
                {
                    Type = "independents",
                    Id = "1",
                    Attributes = new Dictionary<string, object> { { "property", property } },
                    // a common case for this is deserialization in unit tests
                    Relationships = new Dictionary<string, RelationshipData> { { "dependent", new RelationshipData { } } }
                }
            };

            var contentString = JsonConvert.SerializeObject(content);

            // act
            var result = deserializer.Deserialize<Independent>(contentString);

            // assert
            jsonApiContextMock.VerifySet(mock => mock.DocumentMeta = content.Meta);
        }

        private class TestResource : Identifiable
        {
            [Attr("complex-member")]
            public ComplexType ComplexMember { get; set; }

            [Attr("immutable", isImmutable: true)]
            public string Immutable { get; set; }
        }

        private class TestResourceWithList : Identifiable
        {
            [Attr("complex-members")]
            public List<ComplexType> ComplexMembers { get; set; }
        }

        private class ComplexType
        {
            public string CompoundName { get; set; }
        }

        private class Independent : Identifiable
        {
            [Attr("property")] public string Property { get; set; }
            [HasOne("dependent")] public Dependent Dependent { get; set; }
        }

        private class Dependent : Identifiable
        {
            [HasOne("independent")] public Independent Independent { get; set; }
            public int IndependentId { get; set; }
        }



        [Fact]
        public void Can_Deserialize_Object_With_HasManyRelationship()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<OneToManyIndependent>("independents");
            contextGraphBuilder.AddResource<OneToManyDependent>("dependents");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.HasManyRelationshipPointers).Returns(new HasManyRelationshipPointers());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var contentString =
            @"{
                ""data"": {
                    ""type"": ""independents"",
                    ""id"": ""1"",
                    ""attributes"": { },
                    ""relationships"": {
                        ""dependents"": {
                            ""data"": [
                                {
                                    ""type"": ""dependents"",
                                    ""id"": ""2""
                                }
                            ]
                        }
                    }
                }
            }";

            // act
            var result = deserializer.Deserialize<OneToManyIndependent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Dependents);
            Assert.NotEmpty(result.Dependents);
            Assert.Equal(1, result.Dependents.Count);

            var dependent = result.Dependents[0];
            Assert.Equal(2, dependent.Id);
        }

        private class OneToManyDependent : Identifiable
        {
            [HasOne("independent")] public OneToManyIndependent Independent { get; set; }
            public int IndependentId { get; set; }
        }

        private class OneToManyIndependent : Identifiable
        {
            [HasMany("dependents")] public List<OneToManyDependent> Dependents { get; set; }
        }
    }
}
