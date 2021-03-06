using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DiscoveryTests
{
    public sealed class ServiceDiscoveryFacadeTests
    {
        private static readonly NullLoggerFactory LoggerFactory = NullLoggerFactory.Instance;
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly JsonApiOptions _options = new JsonApiOptions();
        private readonly ResourceGraphBuilder _resourceGraphBuilder;

        public ServiceDiscoveryFacadeTests()
        {
            var dbResolverMock = new Mock<IDbContextResolver>();
            dbResolverMock.Setup(resolver => resolver.GetContext()).Returns(new Mock<DbContext>().Object);
            _services.AddScoped(_ => dbResolverMock.Object);

            _services.AddSingleton<IJsonApiOptions>(_options);
            _services.AddSingleton<ILoggerFactory>(LoggerFactory);
            _services.AddScoped(_ => new Mock<IJsonApiRequest>().Object);
            _services.AddScoped(_ => new Mock<ITargetedFields>().Object);
            _services.AddScoped(_ => new Mock<IResourceGraph>().Object);
            _services.AddScoped(_ => new Mock<IGenericServiceFactory>().Object);
            _services.AddScoped(_ => new Mock<IResourceContextProvider>().Object);
            _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(ResourceChangeTracker<>));
            _services.AddScoped(_ => new Mock<IResourceFactory>().Object);
            _services.AddScoped(_ => new Mock<IPaginationContext>().Object);
            _services.AddScoped(_ => new Mock<IQueryLayerComposer>().Object);
            _services.AddScoped(_ => new Mock<IResourceRepositoryAccessor>().Object);
            _services.AddScoped(_ => new Mock<IResourceHookExecutorFacade>().Object);

            _resourceGraphBuilder = new ResourceGraphBuilder(_options, LoggerFactory);
        }

        [Fact]
        public void Can_add_resources_from_assembly_to_graph()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddAssembly(typeof(Person).Assembly);

            // Act
            facade.DiscoverResources();

            // Assert
            IResourceGraph resourceGraph = _resourceGraphBuilder.Build();

            ResourceContext personResource = resourceGraph.GetResourceContext(typeof(Person));
            personResource.Should().NotBeNull();

            ResourceContext articleResource = resourceGraph.GetResourceContext(typeof(Article));
            articleResource.Should().NotBeNull();
        }

        [Fact]
        public void Can_add_resource_from_current_assembly_to_graph()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddCurrentAssembly();

            // Act
            facade.DiscoverResources();

            // Assert
            IResourceGraph resourceGraph = _resourceGraphBuilder.Build();

            ResourceContext resource = resourceGraph.GetResourceContext(typeof(TestResource));
            resource.Should().NotBeNull();
        }

        [Fact]
        public void Can_add_resource_service_from_current_assembly_to_container()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddCurrentAssembly();

            // Act
            facade.DiscoverInjectables();

            // Assert
            ServiceProvider services = _services.BuildServiceProvider();

            var resourceService = services.GetRequiredService<IResourceService<TestResource>>();
            resourceService.Should().BeOfType<TestResourceService>();
        }

        [Fact]
        public void Can_add_resource_repository_from_current_assembly_to_container()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddCurrentAssembly();

            // Act
            facade.DiscoverInjectables();

            // Assert
            ServiceProvider services = _services.BuildServiceProvider();

            var resourceRepository = services.GetRequiredService<IResourceRepository<TestResource>>();
            resourceRepository.Should().BeOfType<TestResourceRepository>();
        }

        [Fact]
        public void Can_add_resource_definition_from_current_assembly_to_container()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddCurrentAssembly();

            // Act
            facade.DiscoverInjectables();

            // Assert
            ServiceProvider services = _services.BuildServiceProvider();

            var resourceDefinition = services.GetRequiredService<IResourceDefinition<TestResource>>();
            resourceDefinition.Should().BeOfType<TestResourceDefinition>();
        }

        [Fact]
        public void Can_add_resource_hooks_definition_from_current_assembly_to_container()
        {
            // Arrange
            var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, _options, LoggerFactory);
            facade.AddCurrentAssembly();

            _options.EnableResourceHooks = true;

            // Act
            facade.DiscoverInjectables();

            // Assert
            ServiceProvider services = _services.BuildServiceProvider();

            var resourceHooksDefinition = services.GetRequiredService<ResourceHooksDefinition<TestResource>>();
            resourceHooksDefinition.Should().BeOfType<TestResourceHooksDefinition>();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class TestResource : Identifiable
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class TestResourceService : JsonApiResourceService<TestResource>
        {
            public TestResourceService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
                IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
                IResourceChangeTracker<TestResource> resourceChangeTracker, IResourceHookExecutorFacade hookExecutor)
                : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, hookExecutor)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class TestResourceRepository : EntityFrameworkCoreRepository<TestResource>
        {
            public TestResourceRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
                IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
                : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class TestResourceHooksDefinition : ResourceHooksDefinition<TestResource>
        {
            public TestResourceHooksDefinition(IResourceGraph resourceGraph)
                : base(resourceGraph)
            {
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public sealed class TestResourceDefinition : JsonApiResourceDefinition<TestResource>
        {
            public TestResourceDefinition(IResourceGraph resourceGraph)
                : base(resourceGraph)
            {
            }
        }
    }
}
