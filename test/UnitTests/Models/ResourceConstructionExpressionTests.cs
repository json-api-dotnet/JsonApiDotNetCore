using System;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests.Models
{
    public sealed class ResourceConstructionExpressionTests
    {
        [Fact]
        public void When_resource_has_default_constructor_it_must_succeed()
        {
            // Arrange
            var factory = new ResourceFactory(new ServiceContainer());

            // Act
            NewExpression newExpression = factory.CreateNewExpression(typeof(ResourceWithoutConstructor));

            // Assert
            var function = Expression
                .Lambda<Func<ResourceWithoutConstructor>>(newExpression)
                .Compile();
            
            ResourceWithoutConstructor resource = function();
            Assert.NotNull(resource);
        }

        [Fact]
        public void When_resource_has_constructor_with_injectable_parameter_it_must_succeed()
        {
            // Arrange
            var contextOptions = new DbContextOptionsBuilder<AppDbContext>().Options;
            var systemClock = new FrozenSystemClock();
            var appDbContext = new AppDbContext(contextOptions, systemClock);
            
            using var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(DbContextOptions<AppDbContext>), contextOptions);
            serviceContainer.AddService(typeof(ISystemClock), systemClock);
            serviceContainer.AddService(typeof(AppDbContext), appDbContext);

            var factory = new ResourceFactory(serviceContainer);

            // Act
            NewExpression newExpression = factory.CreateNewExpression(typeof(ResourceWithDbContextConstructor));

            // Assert
            var function = Expression
                .Lambda<Func<ResourceWithDbContextConstructor>>(newExpression)
                .Compile();

            ResourceWithDbContextConstructor resource = function();
            Assert.NotNull(resource);
            Assert.Equal(appDbContext, resource.AppDbContext);
        }

        [Fact]
        public void When_resource_has_constructor_with_string_parameter_it_must_fail()
        {
            // Arrange
            var factory = new ResourceFactory(new ServiceContainer());

            // Act
            Action action = () => factory.CreateNewExpression(typeof(ResourceWithStringConstructor));

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(
                "Failed to create an instance of 'UnitTests.Models.ResourceWithStringConstructor': Parameter 'text' could not be resolved.",
                exception.Message);
        }
    }
}
