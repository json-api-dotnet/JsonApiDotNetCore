using System;
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
            var assembly = GetType().Assembly;
            var openGeneric = typeof(IGenericInterface<>);
            var genericArg = typeof(int);

            var expectedImplementation = typeof(Implementation);
            var expectedInterface = typeof(IGenericInterface<int>);

            // Act
            var result = TypeLocator.GetGenericInterfaceImplementation(
                assembly,
                openGeneric,
                genericArg
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedImplementation, result.Value.implementation);
            Assert.Equal(expectedInterface, result.Value.registrationInterface);
        }

        [Fact]
        public void GetDerivedGenericTypes_Gets_Implementation()
        {
            // Arrange
            var assembly = GetType().Assembly;
            var openGeneric = typeof(BaseType<>);
            var genericArg = typeof(int);

            var expectedImplementation = typeof(DerivedType);

            // Act
            var results = TypeLocator.GetDerivedGenericTypes(
                assembly,
                openGeneric,
                genericArg
            );

            // Assert
            Assert.NotNull(results);
            var result = Assert.Single(results);
            Assert.Equal(expectedImplementation, result);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_JsonApiResource()
        {
            // Arrange
            var type = typeof(Model);

            // Act
            var idType = TypeLocator.TryGetIdType(type);

            // Assert
            Assert.Equal(typeof(int), idType);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_NonJsonApiResource()
        {
            // Arrange
            var type = typeof(DerivedType);

            // Act
            var idType = TypeLocator.TryGetIdType(type);

            // Assert
            Assert.Null(idType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_Type_If_Type_Is_IIdentifiable()
        {
            // Arrange
            var resourceType = typeof(Model);

            // Act
            var descriptor = TypeLocator.TryGetResourceDescriptor(resourceType);

            // Assert
            Assert.NotNull(descriptor);
            Assert.Equal(resourceType, descriptor.ResourceType);
            Assert.Equal(typeof(int), descriptor.IdType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_False_If_Type_Is_IIdentifiable()
        {
            // Arrange
            var resourceType = typeof(String);

            // Act
            var descriptor = TypeLocator.TryGetResourceDescriptor(resourceType);

            // Assert
            Assert.Null(descriptor);
        }
    }
}
