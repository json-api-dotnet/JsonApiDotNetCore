using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
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
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions {
                JsonContractResolver  = new CamelCasePropertyNamesContractResolver()
            });

            var genericProcessorFactoryMock = new Mock<IGenericProcessorFactory>();

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object, genericProcessorFactoryMock.Object);

            var content = new Document {
                Data = new DocumentData {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object> {
                        { 
                            "complex-member", new { compoundName = "testName" } 
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
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions {
                JsonContractResolver  = new CamelCasePropertyNamesContractResolver()
            });

            var genericProcessorFactoryMock = new Mock<IGenericProcessorFactory>();

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object, genericProcessorFactoryMock.Object);

            var content = new Document {
                Data = new DocumentData {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object> {
                        { 
                            "complex-members", new [] { 
                                new { compoundName = "testName" }
                            } 
                        }
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

            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions {
                JsonContractResolver  = new DasherizedResolver() // <---
            });

            var genericProcessorFactoryMock = new Mock<IGenericProcessorFactory>();

            var deserializer = new JsonApiDeSerializer(jsonApiContextMock.Object, genericProcessorFactoryMock.Object);

            var content = new Document {
                Data = new DocumentData {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object> {
                        { 
                            "complex-member", new Dictionary<string, string> { { "compound-name",  "testName" } }
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

        private class TestResource : Identifiable
        {
            [Attr("complex-member")]
            public ComplexType ComplexMember { get; set; }
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
    }
}
