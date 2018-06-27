using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Services.Operations.Processors;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class CreateOpProcessorTests
    {
        private readonly Mock<ICreateService<TestResource>> _createServiceMock;
        private readonly Mock<IJsonApiDeSerializer> _deserializerMock;
        private readonly Mock<IDocumentBuilder> _documentBuilderMock;

        public CreateOpProcessorTests()
        {
            _createServiceMock = new Mock<ICreateService<TestResource>>();
            _deserializerMock = new Mock<IJsonApiDeSerializer>();
            _documentBuilderMock = new Mock<IDocumentBuilder>();
        }

        [Fact]
        public async Task ProcessAsync_Deserializes_And_Creates()
        {
            // arrange
            var testResource = new TestResource {
                Name = "some-name"
            };

            var data = new DocumentData {
                Type = "test-resources",
                Attributes = new Dictionary<string, object> {
                    { "name", testResource.Name }
                }
            };

            var operation = new Operation {
                Data = data,                
            };

            var contextGraph = new ContextGraphBuilder()
                .AddResource<TestResource>("test-resources")
                .Build();

            _deserializerMock.Setup(m => m.DocumentToObject(It.IsAny<DocumentData>(), It.IsAny<List<DocumentData>>()))
                .Returns(testResource);

            var opProcessor = new CreateOpProcessor<TestResource>(
                _createServiceMock.Object,
                _deserializerMock.Object,
                _documentBuilderMock.Object,
                contextGraph
            );

            _documentBuilderMock.Setup(m => m.GetData(It.IsAny<ContextEntity>(), It.IsAny<TestResource>()))
                .Returns(data);

            // act
            var result = await opProcessor.ProcessAsync(operation);

            // assert
            Assert.Equal(OperationCode.add, result.Op);
            Assert.NotNull(result.Data);
            Assert.Equal(testResource.Name, result.DataObject.Attributes["name"]);
            _createServiceMock.Verify(m => m.CreateAsync(It.IsAny<TestResource>()));
        }

        public class TestResource : Identifiable
        {
            [Attr("name")]
            public string Name { get; set; }
        }
    }
}
