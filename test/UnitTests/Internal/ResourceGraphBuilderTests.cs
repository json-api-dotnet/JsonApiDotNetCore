#nullable disable

using System;
using System.Linq;
using Castle.DynamicProxy;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class ResourceGraphBuilderTests
    {
        [Fact]
        public void Throws_when_adding_resource_type_that_implements_only_non_generic_IIdentifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);

            // Act
            Action action = () => resourceGraphBuilder.Add(typeof(ResourceWithoutId));

            // Assert
            action.Should().ThrowExactly<InvalidConfigurationException>()
                .WithMessage($"Resource type '{typeof(ResourceWithoutId)}' implements 'IIdentifiable', but not 'IIdentifiable<TId>'.");
        }

        [Fact]
        public void Logs_warning_when_adding_non_resource_type()
        {
            // Arrange
            var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), loggerFactory);

            // Act
            resourceGraphBuilder.Add(typeof(NonResource));

            // Assert
            loggerFactory.Logger.Messages.Should().HaveCount(1);

            FakeLoggerFactory.FakeLogMessage message = loggerFactory.Logger.Messages.ElementAt(0);
            message.LogLevel.Should().Be(LogLevel.Warning);
            message.Text.Should().Be($"Skipping: Type '{typeof(NonResource)}' does not implement 'IIdentifiable'.");
        }

        [Fact]
        public void Can_resolve_correct_type_for_lazy_loading_proxy()
        {
            // Arrange
            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<ResourceOfInt32>().Build();
            var proxyGenerator = new ProxyGenerator();
            var proxy = proxyGenerator.CreateClassProxy<ResourceOfInt32>();

            // Act
            ResourceType resourceType = resourceGraph.GetResourceType(proxy.GetType());

            // Assert
            resourceType.ClrType.Should().Be(typeof(ResourceOfInt32));
        }

        [Fact]
        public void Can_resolve_correct_type_for_resource()
        {
            // Arrange
            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<ResourceOfInt32>().Build();

            // Act
            ResourceType resourceType = resourceGraph.GetResourceType(typeof(ResourceOfInt32));

            // Assert
            resourceType.ClrType.Should().Be(typeof(ResourceOfInt32));
        }

        private sealed class ResourceWithoutId : IIdentifiable
        {
            public string StringId { get; set; }
            public string LocalId { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class NonResource
        {
        }

        // ReSharper disable once ClassCanBeSealed.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public class ResourceOfInt32 : Identifiable<int>
        {
        }
    }
}
