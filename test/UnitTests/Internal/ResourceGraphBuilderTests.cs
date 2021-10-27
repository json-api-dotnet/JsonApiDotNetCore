using System.Linq;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class ResourceGraphBuilderTests
    {
        [Fact]
        public void AddDbContext_Does_Not_Throw_If_Context_Contains_Members_That_Do_Not_Implement_IIdentifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);

            // Act
            resourceGraphBuilder.Add(typeof(TestDbContext));
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();

            // Assert
            Assert.Empty(resourceGraph.GetResourceTypes());
        }

        [Fact]
        public void Adding_DbContext_Members_That_Do_Not_Implement_IIdentifiable_Logs_Warning()
        {
            // Arrange
            var loggerFactory = new FakeLoggerFactory(LogLevel.Warning);
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), loggerFactory);
            resourceGraphBuilder.Add(typeof(TestDbContext));

            // Act
            resourceGraphBuilder.Build();

            // Assert
            Assert.Single(loggerFactory.Logger.Messages);
            Assert.Equal(LogLevel.Warning, loggerFactory.Logger.Messages.Single().LogLevel);

            Assert.Equal("Entity 'UnitTests.Internal.ResourceGraphBuilderTests+TestDbContext' does not implement 'IIdentifiable'.",
                loggerFactory.Logger.Messages.Single().Text);
        }

        [Fact]
        public void GetResourceType_Yields_Right_Type_For_LazyLoadingProxy()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.Add<Bar>();
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();
            var proxyGenerator = new ProxyGenerator();

            // Act
            var proxy = proxyGenerator.CreateClassProxy<Bar>();
            ResourceType barType = resourceGraph.GetResourceType(proxy.GetType());

            // Assert
            Assert.Equal(typeof(Bar), barType.ClrType);
        }

        [Fact]
        public void GetResourceType_Yields_Right_Type_For_Identifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.Add<Bar>();
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();

            // Act
            ResourceType barType = resourceGraph.GetResourceType(typeof(Bar));

            // Assert
            Assert.Equal(typeof(Bar), barType.ClrType);
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class Foo
        {
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TestDbContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }
        }

        // ReSharper disable once ClassCanBeSealed.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public class Bar : Identifiable
        {
        }
    }
}
