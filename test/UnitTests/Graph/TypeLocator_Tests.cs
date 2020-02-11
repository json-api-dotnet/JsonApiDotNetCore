using System;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Internal
{
    public class TypeLocator_Tests
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
            var (implementation, registrationInterface) = TypeLocator.GetGenericInterfaceImplementation(
                assembly,
                openGeneric,
                genericArg
            );

            // Assert
            Assert.Equal(expectedImplementation, implementation);
            Assert.Equal(expectedInterface, registrationInterface);
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
            var expectedIdType = typeof(int);

            // Act
            var idType = TypeLocator.GetIdType(type);

            // Assert
            Assert.Equal(expectedIdType, idType);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_NonJsonApiResource()
        {
            // Arrange
            var type = typeof(DerivedType);
            Type expectedIdType = null;

            // Act
            var idType = TypeLocator.GetIdType(type);

            // Assert
            Assert.Equal(expectedIdType, idType);
        }

        [Fact]
        public void GetIdentifiableTypes_Locates_Identifiable_Resource()
        {
            // Arrange
            var resourceType = typeof(Model);

            // Act
            var results = TypeLocator.GetIdentifiableTypes(resourceType.Assembly);

            // Assert
            Assert.Contains(results, r => r.ResourceType == resourceType);
        }

        [Fact]
        public void GetIdentifiableTypes_Only_Contains_IIdentifiable_Types()
        {
            // Arrange
            var resourceType = typeof(Model);

            // Act
            var resourceDescriptors = TypeLocator.GetIdentifiableTypes(resourceType.Assembly);

            // Assert
            foreach(var resourceDescriptor in resourceDescriptors)
                Assert.True(typeof(IIdentifiable).IsAssignableFrom(resourceDescriptor.ResourceType));
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_True_If_Type_Is_IIdentifiable()
        {
            // Arrange
            var resourceType = typeof(Model);

            // Act
            var isJsonApiResource = TypeLocator.TryGetResourceDescriptor(resourceType, out var descriptor);

            // Assert
            Assert.True(isJsonApiResource);
            Assert.Equal(resourceType, descriptor.ResourceType);
            Assert.Equal(typeof(int), descriptor.IdType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_False_If_Type_Is_IIdentifiable()
        {
            // Arrange
            var resourceType = typeof(String);

            // Act
            var isJsonApiResource = TypeLocator.TryGetResourceDescriptor(resourceType, out var _);

            // Assert
            Assert.False(isJsonApiResource);
        }
    }

    
    public interface IGenericInterface<T> { }
    public class Implementation : IGenericInterface<int> { }


    public class BaseType<T> { }
    public class DerivedType : BaseType<int> { }

    public class Model : Identifiable { }
}
