using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace DiscoveryTests;

public sealed class ServiceDiscoveryFacadeTests
{
    private readonly ServiceCollection _services = new();

    public ServiceDiscoveryFacadeTests()
    {
        _services.AddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
        _services.AddScoped<IDbContextResolver>(_ => new FakeDbContextResolver());
    }

    [Fact]
    public void Can_add_resources_from_assembly_to_graph()
    {
        // Arrange
        Action<ServiceDiscoveryFacade> addAction = facade => facade.AddAssembly(typeof(Person).Assembly);

        // Act
        _services.AddJsonApi(discovery: facade => addAction(facade));

        // Assert
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();

        ResourceType? personType = resourceGraph.FindResourceType(typeof(Person));
        personType.ShouldNotBeNull();

        ResourceType? todoItemType = resourceGraph.FindResourceType(typeof(TodoItem));
        todoItemType.ShouldNotBeNull();
    }

    [Fact]
    public void Can_add_resource_from_current_assembly_to_graph()
    {
        // Arrange
        Action<ServiceDiscoveryFacade> addAction = facade => facade.AddCurrentAssembly();

        // Act
        _services.AddJsonApi(discovery: facade => addAction(facade));

        // Assert
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();

        ResourceType? resourceType = resourceGraph.FindResourceType(typeof(PrivateResource));
        resourceType.ShouldNotBeNull();
    }

    [Fact]
    public void Can_add_resource_service_from_current_assembly_to_container()
    {
        // Arrange
        Action<ServiceDiscoveryFacade> addAction = facade => facade.AddCurrentAssembly();

        // Act
        _services.AddJsonApi(discovery: facade => addAction(facade));

        // Assert
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var resourceService = serviceProvider.GetRequiredService<IResourceService<PrivateResource, int>>();

        resourceService.Should().BeOfType<PrivateResourceService>();
    }

    [Fact]
    public void Can_add_resource_repository_from_current_assembly_to_container()
    {
        // Arrange
        Action<ServiceDiscoveryFacade> addAction = facade => facade.AddCurrentAssembly();

        // Act
        _services.AddJsonApi(discovery: facade => addAction(facade));

        // Assert
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var resourceRepository = serviceProvider.GetRequiredService<IResourceRepository<PrivateResource, int>>();

        resourceRepository.Should().BeOfType<PrivateResourceRepository>();
    }

    [Fact]
    public void Can_add_resource_definition_from_current_assembly_to_container()
    {
        // Arrange
        Action<ServiceDiscoveryFacade> addAction = facade => facade.AddCurrentAssembly();

        // Act
        _services.AddJsonApi(discovery: facade => addAction(facade));

        // Assert
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var resourceDefinition = serviceProvider.GetRequiredService<IResourceDefinition<PrivateResource, int>>();

        resourceDefinition.Should().BeOfType<PrivateResourceDefinition>();
    }

    private sealed class FakeDbContextResolver : IDbContextResolver
    {
        private readonly FakeDbContextOptions _dbContextOptions = new();

        public DbContext GetContext()
        {
            return new DbContext(_dbContextOptions);
        }

        private sealed class FakeDbContextOptions : DbContextOptions
        {
            public override Type ContextType => typeof(object);

            public override DbContextOptions WithExtension<TExtension>(TExtension extension)
            {
                return this;
            }
        }
    }
}
