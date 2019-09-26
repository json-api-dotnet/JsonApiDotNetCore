using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Request;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Contracts;

using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace UnitTests.Deserialization
{
    public class JsonApiSerializerTests
    {
        [Fact]
        public void Can_Serialize_Complex_Types()
        {
            // arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");

            var serializer = GetSerializer(resourceGraphBuilder);

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
            var resourceGraphBuilder = new ResourceGraphBuilder();
            resourceGraphBuilder.AddResource<TestResource>("test-resource");
            resourceGraphBuilder.AddResource<ChildResource>("children");
            resourceGraphBuilder.AddResource<InfectionResource>("infections");

            var serializer = GetSerializer(
                resourceGraphBuilder,
                new List<string> { "children.infections" }
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
            ResourceGraphBuilder resourceGraphBuilder,
            List<string> included = null)
        {
            var resourceGraph = resourceGraphBuilder.Build();
            var requestManagerMock = new Mock<IRequestManager>();
            requestManagerMock.Setup(m => m.GetRequestResource()).Returns(resourceGraph.GetContextEntity("test-resource"));
            requestManagerMock.Setup(m => m.IncludedRelationships).Returns(included);
            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions());
            jsonApiContextMock.Setup(m => m.RequestEntity).Returns(resourceGraph.GetContextEntity("test-resource"));
            jsonApiContextMock.Setup(m => m.RequestManager).Returns(requestManagerMock.Object);


            jsonApiContextMock.Setup(m => m.MetaBuilder).Returns(new MetaBuilder());
            var pmMock = new Mock<IPageManager>();
            jsonApiContextMock.Setup(m => m.PageManager).Returns(pmMock.Object);



            var jsonApiOptions = new JsonApiOptions();
            jsonApiContextMock.Setup(m => m.Options).Returns(jsonApiOptions);

            var services = new ServiceCollection();

            var mvcBuilder = services.AddMvcCore();

            services
                .AddJsonApiInternals(jsonApiOptions);

            var provider = services.BuildServiceProvider();
            var scoped = new TestScopedServiceProvider(provider);

            var documentBuilder = GetDocumentBuilder(jsonApiContextMock, requestManagerMock.Object, scopedServiceProvider: scoped);
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
            [HasMany("infections")] public List<InfectionResource> Infections { get; set; }

            [HasOne("parent")] public TestResource Parent { get; set; }
        }

        private class InfectionResource : Identifiable
        {
            [HasOne("infected")] public ChildResource Infected { get; set; }
        }

        private DocumentBuilder GetDocumentBuilder(Mock<IJsonApiContext> jaContextMock, IRequestManager requestManager, TestScopedServiceProvider scopedServiceProvider = null)
        {
            var pageManagerMock = new Mock<IPageManager>();

            return new DocumentBuilder(jaContextMock.Object, pageManagerMock.Object, requestManager, scopedServiceProvider: scopedServiceProvider);

        }
    }
}
