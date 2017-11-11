using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
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
    public class JsonApiSerializerTests
    {
        [Fact]
        public void Can_Serialize_Complex_Types()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();
            contextGraphBuilder.AddResource<TestResource>("test-resource");
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContextMock = new Mock<IJsonApiContext>();
            jsonApiContextMock.SetupAllProperties();
            jsonApiContextMock.Setup(m => m.ContextGraph).Returns(contextGraph);
            jsonApiContextMock.Setup(m => m.Options).Returns(new JsonApiOptions());
            jsonApiContextMock.Setup(m => m.RequestEntity)
                .Returns(contextGraph.GetContextEntity("test-resource"));
            jsonApiContextMock.Setup(m => m.MetaBuilder).Returns(new MetaBuilder());
            jsonApiContextMock.Setup(m => m.PageManager).Returns(new PageManager());

            var documentBuilder = new DocumentBuilder(jsonApiContextMock.Object);
            var serializer = new JsonApiSerializer(jsonApiContextMock.Object, documentBuilder);
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
            Assert.Equal("{\"data\":{\"type\":\"test-resource\",\"id\":\"\",\"attributes\":{\"complex-member\":{\"compound-name\":\"testname\"}}}}", result);
        }

        private class TestResource : Identifiable
        {
            [Attr("complex-member")]
            public ComplexType ComplexMember { get; set; }
        }

        private class ComplexType
        {
            public string CompoundName { get; set; }
        }
    }
}
