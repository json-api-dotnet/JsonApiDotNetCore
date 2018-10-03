using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Services.Operations;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class OperationProcessorResolverTests
    {
        private readonly Mock<IGenericProcessorFactory> _processorFactoryMock;
        public readonly Mock<IJsonApiContext> _jsonApiContextMock;

        public OperationProcessorResolverTests()
        {
            _processorFactoryMock = new Mock<IGenericProcessorFactory>();
            _jsonApiContextMock = new Mock<IJsonApiContext>();
        }

        [Fact]
        public void LocateCreateService_Throws_400_For_Entity_Not_Registered()
        {
            // arrange
            _jsonApiContextMock.Setup(m => m.ContextGraph).Returns(new ContextGraphBuilder().Build());
            var service = GetService();
            var op = new Operation
            {
                Ref = new ResourceReference
                {
                    Type = "non-existent-type"
                }
            };

            // act, assert
            var e = Assert.Throws<JsonApiException>(() => service.LocateCreateService(op));
            Assert.Equal(400, e.GetStatusCode());
        }

        [Fact]
        public void LocateGetService_Throws_400_For_Entity_Not_Registered()
        {
            // arrange
            _jsonApiContextMock.Setup(m => m.ContextGraph).Returns(new ContextGraphBuilder().Build());
            var service = GetService();
            var op = new Operation
            {
                Ref = new ResourceReference
                {
                    Type = "non-existent-type"
                }
            };

            // act, assert
            var e = Assert.Throws<JsonApiException>(() => service.LocateGetService(op));
            Assert.Equal(400, e.GetStatusCode());
        }

        [Fact]
        public void LocateRemoveService_Throws_400_For_Entity_Not_Registered()
        {
            // arrange
            _jsonApiContextMock.Setup(m => m.ContextGraph).Returns(new ContextGraphBuilder().Build());
            var service = GetService();
            var op = new Operation
            {
                Ref = new ResourceReference
                {
                    Type = "non-existent-type"
                }
            };

            // act, assert
            var e = Assert.Throws<JsonApiException>(() => service.LocateRemoveService(op));
            Assert.Equal(400, e.GetStatusCode());
        }

        [Fact]
        public void LocateUpdateService_Throws_400_For_Entity_Not_Registered()
        {
            // arrange
            _jsonApiContextMock.Setup(m => m.ContextGraph).Returns(new ContextGraphBuilder().Build());
            var service = GetService();
            var op = new Operation
            {
                Ref = new ResourceReference
                {
                    Type = "non-existent-type"
                }
            };

            // act, assert
            var e = Assert.Throws<JsonApiException>(() => service.LocateUpdateService(op));
            Assert.Equal(400, e.GetStatusCode());
        }

        private OperationProcessorResolver GetService()
            => new OperationProcessorResolver(_processorFactoryMock.Object, _jsonApiContextMock.Object);
    }
}
