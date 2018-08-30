using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Moq;
using Xunit;

namespace UnitTests.Serialization
{
    public class JsonApiSerializerTests
    {
        [Fact]
        public void Can_Serialize_Complex_Types()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resource");
          
            var serializer = GetSerializer(contextGraphBuilder);

            var resource = new TestResource
            {
                ComplexMember = new ComplexType
                {
                    CompoundName = "testname"
                }
            };

            // act
            var result = serializer.Serialize(resource);

            // assert
            Assert.NotNull(result);

            var expectedFormatted = 
            @"{
                ""data"": {
                    ""attributes"": {
                        ""complex-member"": {
                            ""compound-name"": ""testname""
                        }
                    },
                    ""relationships"": {
                        ""children"": {
                            ""links"": {
                                ""self"": ""/test-resource//relationships/children"",
                                ""related"": ""/test-resource//children""
                            }
                        }
                    },
                    ""type"": ""test-resource"",
                    ""id"": """"
                }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Can_Serialize_Deeply_Nested_Relationships()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resources");
            contextGraphBuilder.AddResource<ChildResource>("children");
            contextGraphBuilder.AddResource<InfectionResource>("infections");
          
            var serializer = GetSerializer(
                contextGraphBuilder,
                included: new List<string> { "children.infections" }
            );

            var resource = new TestResource
            {
                Children = new List<ChildResource> {
                    new ChildResource {
                        Infections = new List<InfectionResource> {
                            new InfectionResource(),
                            new InfectionResource(),
                        }
                    },
                    new ChildResource()
                }
            };

            // act
            var result = serializer.Serialize(resource);

            // assert
            Assert.NotNull(result);
            
            var expectedFormatted = 
            @"{
                ""data"": {
                    ""attributes"": {
                        ""complex-member"": {
                            ""compound-name"": ""testname""
                        }
                    },
                    ""relationships"": {
                        ""children"": {
                            ""links"": {
                                ""self"": ""/test-resource//relationships/children"",
                                ""related"": ""/test-resource//children""
                            }
                        }
                    },
                    ""type"": ""test-resource"",
                    ""id"": """"
                },
                ""included"": {
                    {
                        ""attributes"": {},
                        ""relationships"": {},
                        ""type"": ""children""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {},
                        ""type"": ""children""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {},
                        ""type"": ""infections""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {},
                        ""type"": ""infections""
                    }
                }
            }";
            var expected = Regex.Replace(expectedFormatted, @"\s+", "");

            Assert.Equal(expected, result);
        }

        private JsonApiSerializer GetSerializer(
            ContextGraphBuilder contextGraphBuilder, 
            List<string> included = null)
        {
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions());
            jsonApiContextMock.Setup(m => m.RequestEntity).Returns(contextGraph.GetContextEntity("test-resource"));
            // jsonApiContextMock.Setup(m => m.AttributesToUpdate).Returns(new Dictionary<AttrAttribute, object>());
            // jsonApiContextMock.Setup(m => m.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>());
            // jsonApiContextMock.Setup(m => m.HasManyRelationshipPointers).Returns(new HasManyRelationshipPointers());
            // jsonApiContextMock.Setup(m => m.HasOneRelationshipPointers).Returns(new HasOneRelationshipPointers());
            jsonApiContextMock.Setup(m => m.MetaBuilder).Returns(new MetaBuilder());
            jsonApiContextMock.Setup(m => m.PageManager).Returns(new PageManager());

            if (included != null)
                jsonApiContextMock.Setup(m => m.IncludedRelationships).Returns(included);

            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var documentBuilder = new DocumentBuilder(jsonApiContextMock.Object);
            var serializer = new JsonApiSerializer(jsonApiContextMock.Object, documentBuilder);

            return serializer;
        }

        private class TestResource : Identifiable
        {
            [Attr("complex-member")]
            public ComplexType ComplexMember { get; set; }

            [HasMany("children")] public List<ChildResource> Children { get; set; }
        }

        private class ComplexType
        {
            public string CompoundName { get; set; }
        }

        private class ChildResource : Identifiable
        {
            [HasMany("infections")] public List<InfectionResource> Infections { get; set;}

            [HasOne("parent")] public TestResource Parent { get; set; }
        }

        private class InfectionResource : Identifiable
        {
            [HasOne("infected")] public ChildResource Infected { get; set; }
        }
    }
}
