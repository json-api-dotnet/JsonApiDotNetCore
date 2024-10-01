using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;
using Xunit;

namespace UnitTests.Graph;

public sealed class TypeLocatorTests
{
    [Fact]
    public void GetContainerRegistrationFromAssembly_Gets_Implementation()
    {
        // Arrange
        Assembly assembly = GetType().Assembly;
        Type unboundInterface = typeof(IGenericInterface<>);
        Type typeArgument = typeof(int);

        var typeLocator = new TypeLocator();

        // Act
        (Type implementationType, Type serviceInterface)? result = typeLocator.GetContainerRegistrationFromAssembly(assembly, unboundInterface, typeArgument);

        // Assert
        result.ShouldNotBeNull();
        result.Value.implementationType.Should().Be(typeof(Implementation));
        result.Value.serviceInterface.Should().Be(typeof(IGenericInterface<int>));
    }

    [Fact]
    public void GetIdType_Correctly_Identifies_JsonApiResource()
    {
        // Arrange
        Type type = typeof(Model);

        var typeLocator = new TypeLocator();

        // Act
        Type? idType = typeLocator.LookupIdType(type);

        // Assert
        idType.Should().Be(typeof(int));
    }

    [Fact]
    public void GetIdType_Correctly_Identifies_NonJsonApiResource()
    {
        // Arrange
        Type type = typeof(DerivedType);

        var typeLocator = new TypeLocator();

        // Act
        Type? idType = typeLocator.LookupIdType(type);

        // Assert
        idType.Should().BeNull();
    }

    [Fact]
    public void ResolveResourceDescriptor_Returns_Type_If_Type_Is_IIdentifiable()
    {
        // Arrange
        Type resourceClrType = typeof(Model);

        var typeLocator = new TypeLocator();

        // Act
        ResourceDescriptor? descriptor = typeLocator.ResolveResourceDescriptor(resourceClrType);

        // Assert
        descriptor.ShouldNotBeNull();
        descriptor.ResourceClrType.Should().Be(resourceClrType);
        descriptor.IdClrType.Should().Be(typeof(int));
    }

    [Fact]
    public void ResolveResourceDescriptor_Returns_False_If_Type_Is_IIdentifiable()
    {
        // Arrange
        Type resourceClrType = typeof(string);

        var typeLocator = new TypeLocator();

        // Act
        ResourceDescriptor? descriptor = typeLocator.ResolveResourceDescriptor(resourceClrType);

        // Assert
        descriptor.Should().BeNull();
    }
}
