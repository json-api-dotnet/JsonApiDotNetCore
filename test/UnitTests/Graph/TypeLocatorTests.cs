using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;
using Xunit;

namespace UnitTests.Graph
{
    public sealed class TypeLocatorTests
    {
        [Fact]
        public void GetGenericInterfaceImplementation_Gets_Implementation()
        {
            // Arrange
            Assembly assembly = GetType().Assembly;
            Type openGeneric = typeof(IGenericInterface<>);
            Type genericArg = typeof(int);

            Type expectedImplementation = typeof(Implementation);
            Type expectedInterface = typeof(IGenericInterface<int>);

            var typeLocator = new TypeLocator();

            // Act
            (Type implementation, Type registrationInterface)? result = typeLocator.GetGenericInterfaceImplementation(assembly, openGeneric, genericArg);

            // Assert
            result.ShouldNotBeNull();
            result.Value.implementation.Should().Be(expectedImplementation);
            result.Value.registrationInterface.Should().Be(expectedInterface);
        }

        [Fact]
        public void GetDerivedGenericTypes_Gets_Implementation()
        {
            // Arrange
            Assembly assembly = GetType().Assembly;
            Type openGeneric = typeof(BaseType<>);
            Type genericArg = typeof(int);

            Type expectedImplementation = typeof(DerivedType);

            var typeLocator = new TypeLocator();

            // Act
            IReadOnlyCollection<Type> results = typeLocator.GetDerivedGenericTypes(assembly, openGeneric, genericArg);

            // Assert
            results.ShouldHaveCount(1);
            results.ElementAt(0).Should().Be(expectedImplementation);
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
}
