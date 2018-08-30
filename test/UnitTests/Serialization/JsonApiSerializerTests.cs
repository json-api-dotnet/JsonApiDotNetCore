using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
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
            contextGraphBuilder.AddResource<TestResource>("test-resource");
            contextGraphBuilder.AddResource<ChildResource>("children");
            contextGraphBuilder.AddResource<InfectionResource>("infections");
          
            var serializer = GetSerializer(
                contextGraphBuilder,
                included: new List<string> { "children.infections" }
            );

            var resource = new TestResource
            {
                Id = 1,
                Children = new List<ChildResource> {
                    new ChildResource {
                        Id = 2,
                        Infections = new List<InfectionResource> {
                            new InfectionResource { Id = 4 },
                            new InfectionResource { Id = 5 },
                        }
                    },
                    new ChildResource {
                        Id = 3
                    }
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
                        ""complex-member"": null
                    },
                    ""relationships"": {
                        ""children"": {
                            ""links"": {
                                ""self"": ""/test-resource/1/relationships/children"",
                                ""related"": ""/test-resource/1/children""
                            },
                            ""data"": [{
                                ""type"": ""children"",
                                ""id"": ""2""
                            }, {
                                ""type"": ""children"",
                                ""id"": ""3""
                            }]
                        }
                    },
                    ""type"": ""test-resource"",
                    ""id"": ""1""
                },
                ""included"": [
                    {
                        ""attributes"": {},
                        ""relationships"": {
                            ""infections"": {
                                ""links"": {
                                    ""self"": ""/children/2/relationships/infections"",
                                    ""related"": ""/children/2/infections""
                                },
                                ""data"": [{
                                    ""type"": ""infections"",
                                    ""id"": ""4""
                                }, {
                                    ""type"": ""infections"",
                                    ""id"": ""5""
                                }]
                            },
                            ""parent"": {
                                ""links"": {
                                    ""self"": ""/children/2/relationships/parent"",
                                    ""related"": ""/children/2/parent""
                                }
                            }
                        },
                        ""type"": ""children"",
                        ""id"": ""2""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {
                            ""infected"": {
                                ""links"": {
                                    ""self"": ""/infections/4/relationships/infected"",
                                    ""related"": ""/infections/4/infected""
                                }
                            }
                        },
                        ""type"": ""infections"",
                        ""id"": ""4""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {
                            ""infected"": {
                                ""links"": {
                                    ""self"": ""/infections/5/relationships/infected"",
                                    ""related"": ""/infections/5/infected""
                                }
                            }
                        },
                        ""type"": ""infections"",
                        ""id"": ""5""
                    },
                    {
                        ""attributes"": {},
                        ""relationships"": {
                            ""infections"": {
                                ""links"": {
                                    ""self"": ""/children/3/relationships/infections"",
                                    ""related"": ""/children/3/infections""
                                }
                            },
                            ""parent"": {
                                ""links"": {
                                    ""self"": ""/children/3/relationships/parent"",
                                    ""related"": ""/children/3/parent""
                                }
                            }
                        },
                        ""type"": ""children"",
                        ""id"": ""3""
                    }
                ]
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

            var services = new ServiceCollection();

            var mvcBuilder = services.AddMvcCore();

            services
                .AddJsonApiInternals(jsonApiOptions);

            var provider = services.BuildServiceProvider();
            var scoped = new TestScopedServiceProvider(provider);

            var documentBuilder = new DocumentBuilder(jsonApiContextMock.Object, scopedServiceProvider: scoped);
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
