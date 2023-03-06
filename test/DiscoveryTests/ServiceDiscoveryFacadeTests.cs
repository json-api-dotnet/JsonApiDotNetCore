using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
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
using TestBuildingBlocks;
using Xunit;

namespace DiscoveryTests;

public sealed class ServiceDiscoveryFacadeTests
{
    private static readonly ILoggerFactory LoggerFactory = NullLoggerFactory.Instance;
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ResourceGraphBuilder _resourceGraphBuilder;

    public ServiceDiscoveryFacadeTests()
    {
        var dbResolverMock = new Mock<IDbContextResolver>();
        dbResolverMock.Setup(resolver => resolver.GetContext()).Returns(new Mock<DbContext>().Object);
        _services.AddScoped(_ => dbResolverMock.Object);

        IJsonApiOptions options = new JsonApiOptions();

        _services.AddSingleton(options);
        _services.AddSingleton(LoggerFactory);
        _services.AddScoped(_ => new Mock<IJsonApiRequest>().Object);
        _services.AddScoped(_ => new Mock<ITargetedFields>().Object);
        _services.AddScoped(_ => new Mock<IResourceGraph>().Object);
        _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(ResourceChangeTracker<>));
        _services.AddScoped(_ => new Mock<IVersionTracker>().Object);
        _services.AddScoped(_ => new Mock<IResourceFactory>().Object);
        _services.AddScoped(_ => new Mock<IPaginationContext>().Object);
        _services.AddScoped(_ => new Mock<IQueryLayerComposer>().Object);
        _services.AddScoped(_ => new Mock<IResourceRepositoryAccessor>().Object);
        _services.AddScoped(_ => new Mock<IResourceDefinitionAccessor>().Object);

        _resourceGraphBuilder = new ResourceGraphBuilder(options, LoggerFactory);
    }

    [Fact]
    public void Can_add_resources_from_assembly_to_graph()
    {
        // Arrange
        var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, LoggerFactory);
        facade.AddAssembly(typeof(Person).Assembly);

        // Act
        facade.DiscoverResources();

        // Assert
        IResourceGraph resourceGraph = _resourceGraphBuilder.Build();

        ResourceType? personType = resourceGraph.FindResourceType(typeof(Person));
        personType.ShouldNotBeNull();

        ResourceType? todoItemType = resourceGraph.FindResourceType(typeof(TodoItem));
        todoItemType.ShouldNotBeNull();
    }

    [Fact]
    public void Can_add_resource_from_current_assembly_to_graph()
    {
        // Arrange
        var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, LoggerFactory);
        facade.AddCurrentAssembly();

        // Act
        facade.DiscoverResources();

        // Assert
        IResourceGraph resourceGraph = _resourceGraphBuilder.Build();

        ResourceType? testResourceType = resourceGraph.FindResourceType(typeof(PrivateResource));
        testResourceType.ShouldNotBeNull();
    }

    [Fact]
    public void Can_add_resource_service_from_current_assembly_to_container()
    {
        // Arrange
        var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, LoggerFactory);
        facade.AddCurrentAssembly();

        // Act
        facade.DiscoverInjectables();

        // Assert
        ServiceProvider services = _services.BuildServiceProvider();

        var resourceService = services.GetRequiredService<IResourceService<PrivateResource, int>>();
        resourceService.Should().BeOfType<PrivateResourceService>();
    }

    [Fact]
    public void Can_add_resource_repository_from_current_assembly_to_container()
    {
        // Arrange
        var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, LoggerFactory);
        facade.AddCurrentAssembly();

        // Act
        facade.DiscoverInjectables();

        // Assert
        ServiceProvider services = _services.BuildServiceProvider();

        var resourceRepository = services.GetRequiredService<IResourceRepository<PrivateResource, int>>();
        resourceRepository.Should().BeOfType<PrivateResourceRepository>();
    }

    [Fact]
    public void Can_add_resource_definition_from_current_assembly_to_container()
    {
        // Arrange
        var facade = new ServiceDiscoveryFacade(_services, _resourceGraphBuilder, LoggerFactory);
        facade.AddCurrentAssembly();

        // Act
        facade.DiscoverInjectables();

        // Assert
        ServiceProvider services = _services.BuildServiceProvider();

        var resourceDefinition = services.GetRequiredService<IResourceDefinition<PrivateResource, int>>();
        resourceDefinition.Should().BeOfType<PrivateResourceDefinition>();
    }
}
