using System;
using System.ComponentModel.Design;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests.Models
{
    public sealed class ResourceConstructionTests
    {
        [Fact]
        public void When_resource_has_default_constructor_it_must_succeed()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<ResourceWithoutConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ServiceContainer(), new TargetedFields());

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
                .AddResource<ResourceWithThrowingConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ServiceContainer(), new TargetedFields());

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
                "Failed to create an instance of 'UnitTests.Models.ResourceConstructionTests+ResourceWithThrowingConstructor' using its default constructor.",
                exception.Message);
        }

        [Fact]
        public void When_resource_has_constructor_with_injectable_parameter_it_must_succeed()
        {
            // Arrange
            var graph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<ResourceWithDbContextConstructor>()
                .Build();

            var appDbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options, new FrozenSystemClock());

            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(AppDbContext), appDbContext);

            var serializer = new RequestDeserializer(graph, serviceContainer, new TargetedFields());

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
                .AddResource<ResourceWithStringConstructor>()
                .Build();

            var serializer = new RequestDeserializer(graph, new ServiceContainer(), new TargetedFields());

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
                "Failed to create an instance of 'UnitTests.Models.ResourceConstructionTests+ResourceWithStringConstructor' using injected constructor parameters.",
                exception.Message);
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
}
