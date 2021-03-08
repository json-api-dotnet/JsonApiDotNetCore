using System;
using System.Collections.Generic;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
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

            // Act
            (Type implementation, Type registrationInterface)? result = TypeLocator.GetGenericInterfaceImplementation(assembly, openGeneric, genericArg);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedImplementation, result.Value.implementation);
            Assert.Equal(expectedInterface, result.Value.registrationInterface);
        }

        [Fact]
        public void GetDerivedGenericTypes_Gets_Implementation()
        {
            // Arrange
            Assembly assembly = GetType().Assembly;
            Type openGeneric = typeof(BaseType<>);
            Type genericArg = typeof(int);

            Type expectedImplementation = typeof(DerivedType);

            // Act
            IReadOnlyCollection<Type> results = TypeLocator.GetDerivedGenericTypes(assembly, openGeneric, genericArg);

            // Assert
            Assert.NotNull(results);
            Type result = Assert.Single(results);
            Assert.Equal(expectedImplementation, result);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_JsonApiResource()
        {
            // Arrange
            Type type = typeof(Model);

            // Act
            Type idType = TypeLocator.TryGetIdType(type);

            // Assert
            Assert.Equal(typeof(int), idType);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_NonJsonApiResource()
        {
            // Arrange
            Type type = typeof(DerivedType);

            // Act
            Type idType = TypeLocator.TryGetIdType(type);

            // Assert
            Assert.Null(idType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_Type_If_Type_Is_IIdentifiable()
        {
            // Arrange
            Type resourceType = typeof(Model);

            // Act
            ResourceDescriptor descriptor = TypeLocator.TryGetResourceDescriptor(resourceType);

            // Assert
            Assert.NotNull(descriptor);
            Assert.Equal(resourceType, descriptor.ResourceType);
            Assert.Equal(typeof(int), descriptor.IdType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_False_If_Type_Is_IIdentifiable()
        {
            // Arrange
            Type resourceType = typeof(string);

            // Act
            ResourceDescriptor descriptor = TypeLocator.TryGetResourceDescriptor(resourceType);

            // Assert
            Assert.Null(descriptor);
        }
    }
}
