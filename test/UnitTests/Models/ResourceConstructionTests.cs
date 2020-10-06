using System;
using System.ComponentModel.Design;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Models
{
    public sealed class ResourceConstructionTests
    {
        public Mock<IJsonApiRequest> _requestMock;
        public Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        
        public ResourceConstructionTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(mock => mock.HttpContext).Returns(new DefaultHttpContext());
            _requestMock = new Mock<IJsonApiRequest>();
            _requestMock.Setup(mock => mock.Kind).Returns(EndpointKind.Primary);
        }

        [Fact]
        public void When_resource_has_default_constructor_it_must_succeed()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<ResourceWithoutConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ResourceFactory(new ServiceContainer()), new TargetedFields(), _mockHttpContextAccessor.Object, _requestMock.Object);

            var body = new
            {
                data = new
                {
                    id = "1",
                    type = "resourceWithoutConstructors"
                }
            };

            string content = JsonConvert.SerializeObject(body);

            // Act
            object result = serializer.Deserialize(content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ResourceWithoutConstructor), result.GetType());
        }

        [Fact]
        public void When_resource_has_default_constructor_that_throws_it_must_fail()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<ResourceWithThrowingConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ResourceFactory(new ServiceContainer()), new TargetedFields(), _mockHttpContextAccessor.Object, _requestMock.Object);

            var body = new
            {
                data = new
                {
                    id = "1",
                    type = "resourceWithThrowingConstructors"
                }
            };

            string content = JsonConvert.SerializeObject(body);

            // Act
            Action action = () => serializer.Deserialize(content);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(
                "Failed to create an instance of 'UnitTests.Models.ResourceWithThrowingConstructor' using its default constructor.",
                exception.Message);
        }

        [Fact]
        public void When_resource_has_constructor_with_injectable_parameter_it_must_succeed()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<ResourceWithDbContextConstructor>()
                .Build();

            var appDbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options, new FrozenSystemClock());

            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(AppDbContext), appDbContext);

            var serializer = new RequestDeserializer(graph, new ResourceFactory(serviceContainer), new TargetedFields(), _mockHttpContextAccessor.Object, _requestMock.Object);

            var body = new
            {
                data = new
                {
                    id = "1",
                    type = "resourceWithDbContextConstructors"
                }
            };

            string content = JsonConvert.SerializeObject(body);

            // Act
            object result = serializer.Deserialize(content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ResourceWithDbContextConstructor), result.GetType());
            Assert.Equal(appDbContext, ((ResourceWithDbContextConstructor)result).AppDbContext);
        }

        [Fact]
        public void When_resource_has_constructor_with_string_parameter_it_must_fail()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<ResourceWithStringConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ResourceFactory(new ServiceContainer()), new TargetedFields(), _mockHttpContextAccessor.Object, _requestMock.Object);

            var body = new
            {
                data = new
                {
                    id = "1",
                    type = "resourceWithStringConstructors"
                }
            };

            string content = JsonConvert.SerializeObject(body);

            // Act
            Action action = () => serializer.Deserialize(content);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(
                "Failed to create an instance of 'UnitTests.Models.ResourceWithStringConstructor' using injected constructor parameters.",
                exception.Message);
        }
    }

    public class ResourceWithoutConstructor : Identifiable
    {
    }

    public class ResourceWithDbContextConstructor : Identifiable
    {
        public AppDbContext AppDbContext { get; }

        public ResourceWithDbContextConstructor(AppDbContext appDbContext)
        {
            AppDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
        }
    }

    public class ResourceWithThrowingConstructor : Identifiable
    {
        public ResourceWithThrowingConstructor()
        {
            throw new ArgumentException("Failed to initialize.");
        }
    }

    public class ResourceWithStringConstructor : Identifiable
    {
        public string Text { get; }

        public ResourceWithStringConstructor(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
