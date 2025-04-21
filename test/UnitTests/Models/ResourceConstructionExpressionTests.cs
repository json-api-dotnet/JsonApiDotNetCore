using System.ComponentModel.Design;
using System.Linq.Expressions;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Models;

public sealed class ResourceConstructionExpressionTests
{
    [Fact]
    public void When_resource_has_default_constructor_it_must_succeed()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var factory = new ResourceFactory(serviceProvider);

        // Act
        NewExpression newExpression = factory.CreateNewExpression(typeof(ResourceWithoutConstructor));

        // Assert
        Func<ResourceWithoutConstructor> createFunction = Expression.Lambda<Func<ResourceWithoutConstructor>>(newExpression).Compile();
        ResourceWithoutConstructor resource = createFunction();

        resource.Should().NotBeNull();
    }

    [Fact]
    public void When_resource_has_constructor_with_string_parameter_it_must_fail()
    {
        // Arrange
        using var serviceProvider = new ServiceContainer();
        var factory = new ResourceFactory(serviceProvider);

        // Act
        Action action = () => factory.CreateNewExpression(typeof(ResourceWithStringConstructor));

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage($"Failed to create an instance of '{typeof(ResourceWithStringConstructor).FullName}': Parameter 'text' could not be resolved.");
    }

    private sealed class ResourceWithoutConstructor : Identifiable<int>;

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ResourceWithStringConstructor : Identifiable<int>
    {
        public string Text { get; }

        public ResourceWithStringConstructor(string text)
        {
            ArgumentException.ThrowIfNullOrEmpty(text);

            Text = text;
        }
    }
}
