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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new ResourceObject {
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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResourceWithList>("test-resource");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new ResourceObject {
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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new DasherizedResolver(); // <--
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new ResourceObject {
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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            var resourceGraph = resourceGraphBuilder.Build();

            var attributesToUpdate = new Dictionary<AttrAttribute, object>();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(attributesToUpdate);

            var jsonApiOptions = new JsonApiOptions();
            jsonApiOptions.SerializerSettings.ContractResolver = new DasherizedResolver();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var content = new Document
            {
                Data = new ResourceObject {
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
            Assert.Single(attributesToUpdate);

            foreach (var attr in attributesToUpdate)
                Assert.False(attr.Key.IsImmutable);
        }

        [Fact]
        public void Can_Deserialize_Independent_Side_Of_One_To_One_Relationship()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<Independent>("independents");
            resourceGraphBuilder.AddResource<Dependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();
            var content = new Document
            {
                Data = new ResourceObject {
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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<Independent>("independents");
            resourceGraphBuilder.AddResource<Dependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.HasOneRelationshipPointers).Returns(new HasOneRelationshipPointers());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();
            var content = new Document
            {
                Data = new ResourceObject {
                    Type = "independents",
                    Id = "1",
                    Attributes = new Dictionary<string, object> { { "property", property } },
                    // a common case for this is deserialization in unit tests
                    Relationships = new Dictionary<string, RelationshipData> {
                        {
                            "dependent", new RelationshipData
                            {
                                SingleData = new ResourceIdentifierObject("dependents", "1")
                            }
                        }
                    }
                }
            };

            var contentString = JsonConvert.SerializeObject(content);

            // act
            var result = deserializer.Deserialize<Independent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(property, result.Property);
            Assert.NotNull(result.Dependent);
            Assert.Equal(1, result.Dependent.Id);
        }

        [Fact]
        public void Sets_The_DocumentMeta_Property_In_JsonApiContext()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<Independent>("independents");
            resourceGraphBuilder.AddResource<Dependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);


            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var property = Guid.NewGuid().ToString();

            var content = new Document
            {
                Meta = new Dictionary<string, object>() { { "foo", "bar" } },
                Data = new ResourceObject {
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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<OneToManyIndependent>("independents");
            resourceGraphBuilder.AddResource<OneToManyDependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>());
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
            Assert.Single(result.Dependents);

            var dependent = result.Dependents[0];
            Assert.Equal(2, dependent.Id);
        }

        [Fact]
        public void Sets_Attribute_Values_On_Included_HasMany_Relationships()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<OneToManyIndependent>("independents");
            resourceGraphBuilder.AddResource<OneToManyDependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>());
            jsonApiContextMock.Setup(m => m.HasManyRelationshipPointers).Returns(new HasManyRelationshipPointers());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var expectedName = "John Doe";
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
                },
                ""included"": [
                    {
                        ""type"": ""dependents"",
                        ""id"": ""2"",
                        ""attributes"": {
                            ""name"": """ + expectedName + @"""
                        }
                    }
                ]
            }";

            // act
            var result = deserializer.Deserialize<OneToManyIndependent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Dependents);
            Assert.NotEmpty(result.Dependents);
            Assert.Single(result.Dependents);

            var dependent = result.Dependents[0];
            Assert.Equal(2, dependent.Id);
            Assert.Equal(expectedName, dependent.Name);
        }

        [Fact]
        public void Sets_Attribute_Values_On_Included_HasOne_Relationships()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<OneToManyIndependent>("independents");
            resourceGraphBuilder.AddResource<OneToManyDependent>("dependents");
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>());
            jsonApiContextMock.Setup(m => m.HasManyRelationshipPointers).Returns(new HasManyRelationshipPointers());
            jsonApiContextMock.Setup(m => m.HasOneRelationshipPointers).Returns(new HasOneRelationshipPointers());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            var expectedName = "John Doe";
            var contentString =
            @"{
                ""data"": {
                    ""type"": ""dependents"",
                    ""id"": ""1"",
                    ""attributes"": { },
                    ""relationships"": {
                        ""independent"": {
                            ""data"": {
                                ""type"": ""independents"",
                                ""id"": ""2""
                            }
                        }
                    }
                },
                ""included"": [
                    {
                        ""type"": ""independents"",
                        ""id"": ""2"",
                        ""attributes"": {
                            ""name"": """ + expectedName + @"""
                        }
                    }
                ]
            }";

            // act
            var result = deserializer.Deserialize<OneToManyDependent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Independent);
            Assert.Equal(2, result.Independent.Id);
            Assert.Equal(expectedName, result.Independent.Name);
        }


        [Fact]
        public void Can_Deserialize_Nested_Included_HasMany_Relationships()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<OneToManyIndependent>("independents");
            resourceGraphBuilder.AddResource<OneToManyDependent>("dependents");
            resourceGraphBuilder.AddResource<ManyToManyNested>("many-to-manys");

            var deserializer = GetDeserializer(resourceGraphBuilder);
          
            var contentString =
            @"{
                ""data"": {
                    ""type"": ""independents"",
                    ""id"": ""1"",
                    ""attributes"": { },
                    ""relationships"": {
                        ""many-to-manys"": {
                           ""data"": [{
                                ""type"": ""many-to-manys"",
                                ""id"": ""2""
                            }, {
                                ""type"": ""many-to-manys"",
                                ""id"": ""3""
                            }]
                        }
                    }
                },
                ""included"": [
                    {
                        ""type"": ""many-to-manys"",
                        ""id"": ""2"",
                        ""attributes"": {},
                        ""relationships"": {
                            ""dependent"": {
                                ""data"": {
                                    ""type"": ""dependents"",
                                    ""id"": ""4""
                                }
                            },
                            ""independent"": {
                                ""data"": {
                                    ""type"": ""independents"",
                                    ""id"": ""5""
                                }
                            }
                        }
                    },
                    {
                        ""type"": ""many-to-manys"",
                        ""id"": ""3"",
                        ""attributes"": {},
                        ""relationships"": {
                            ""dependent"": {
                                ""data"": {
                                    ""type"": ""dependents"",
                                    ""id"": ""4""
                                }
                            },
                            ""independent"": {
                                ""data"": {
                                    ""type"": ""independents"",
                                    ""id"": ""6""
                                }
                            }
                        }
                    },
                    {
                        ""type"": ""dependents"",
                        ""id"": ""4"",
                        ""attributes"": {},
                        ""relationships"": {
                            ""many-to-manys"": {
                                ""data"": [{
                                    ""type"": ""many-to-manys"",
                                    ""id"": ""2""
                                }, {
                                    ""type"": ""many-to-manys"",
                                    ""id"": ""3""
                                }]
                            }
                        }
                    }
                    ,
                    {
                        ""type"": ""independents"",
                        ""id"": ""5"",
                        ""attributes"": {},
                        ""relationships"": {
                            ""many-to-manys"": {
                                ""data"": [{
                                    ""type"": ""many-to-manys"",
                                    ""id"": ""2""
                                }]
                            }
                        }
                    }
                    ,
                    {
                        ""type"": ""independents"",
                        ""id"": ""6"",
                        ""attributes"": {},
                        ""relationships"": {
                            ""many-to-manys"": {
                                ""data"": [{
                                    ""type"": ""many-to-manys"",
                                    ""id"": ""3""
                                }]
                            }
                        }
                    }
                ]
            }";

            // act
            var result = deserializer.Deserialize<OneToManyIndependent>(contentString);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.ManyToManys);
            Assert.Equal(2, result.ManyToManys.Count);

            // TODO: not sure if this should be a thing that works?
            //       could this cause cycles in the graph?
            // Assert.NotNull(result.ManyToManys[0].Dependent);
            // Assert.NotNull(result.ManyToManys[0].Independent);
            // Assert.NotNull(result.ManyToManys[1].Dependent);
            // Assert.NotNull(result.ManyToManys[1].Independent);

            // Assert.Equal(result.ManyToManys[0].Dependent, result.ManyToManys[1].Dependent);
            // Assert.NotEqual(result.ManyToManys[0].Independent, result.ManyToManys[1].Independent);
        }

        private JsonApiDeSerializer GetDeserializer(ResourceGraphBuilder resourceGraphBuilder)
        {
            var resourceGraph = resourceGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            jsonApiContextMock.Setup(m => m.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>());
            jsonApiContextMock.Setup(m => m.HasManyRelationshipPointers).Returns(new HasManyRelationshipPointers());
            jsonApiContextMock.Setup(m => m.HasOneRelationshipPointers).Returns(new HasOneRelationshipPointers());

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object);

            return deserializer;
        }

        private class ManyToManyNested : Identifiable
        {
            [Attr("name")] public string Name { get; set; }
            [HasOne("dependent")] public OneToManyDependent Dependent { get; set; }
            public int DependentId { get; set; }
            [HasOne("independent")] public OneToManyIndependent Independent { get; set; }
            public int InependentId { get; set; }
        }

        private class OneToManyDependent : Identifiable
        {
            [Attr("name")] public string Name { get; set; }
            [HasOne("independent")] public OneToManyIndependent Independent { get; set; }
            public int IndependentId { get; set; }

            [HasMany("many-to-manys")] public List<ManyToManyNested> ManyToManys { get; set; }
        }

        private class OneToManyIndependent : Identifiable
        {
            [Attr("name")] public string Name { get; set; }
            [HasMany("dependents")] public List<OneToManyDependent> Dependents { get; set; }

            [HasMany("many-to-manys")] public List<ManyToManyNested> ManyToManys { get; set; }
        }
    }
}
