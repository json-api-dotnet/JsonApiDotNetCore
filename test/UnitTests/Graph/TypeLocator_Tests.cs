using System;
using System.Reflection;
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
            // arrange
            var assembly = GetType().Assembly;
            var openGeneric = typeof(BaseType<>);
            var genericArg = typeof(int);

            var expectedImplementation = typeof(DerivedType);

            // act
            var results = TypeLocator.GetDerivedGenericTypes(
                assembly,
                openGeneric,
                genericArg
            );

            // assert
            Assert.NotNull(results);
            var result = Assert.Single(results);
            Assert.Equal(expectedImplementation, result);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_JsonApiResource()
        {
            // Arrange
            var type = typeof(Model);
            var exextedIdType = typeof(int);

            // Act
            var (isJsonApiResource, idType) = TypeLocator.GetIdType(type);

            // Assert
            Assert.True(isJsonApiResource);
            Assert.Equal(exextedIdType, idType);
        }

        [Fact]
        public void GetIdType_Correctly_Identifies_NonJsonApiResource()
        {
            // arrange
            var type = typeof(DerivedType);
            Type exextedIdType = null;

            // act
            var (isJsonApiResource, idType) = TypeLocator.GetIdType(type);

            // Assert
            Assert.False(isJsonApiResource);
            Assert.Equal(exextedIdType, idType);
        }

        [Fact]
        public void GetIdentifableTypes_Locates_Identifiable_Resource()
        {
            // arrange
            var resourceType = typeof(Model);

            // act
            var results = TypeLocator.GetIdentifableTypes(resourceType.Assembly);

            // assert
            Assert.Contains(results, r => r.ResourceType == resourceType);
        }

        [Fact]
        public void GetIdentifableTypes__Only_Contains_IIdentifiable_Types()
        {
            // arrange
            var resourceType = typeof(Model);

            // act
            var resourceDescriptors = TypeLocator.GetIdentifableTypes(resourceType.Assembly);

            // assert
            foreach(var resourceDescriptor in resourceDescriptors)
                Assert.True(typeof(IIdentifiable).IsAssignableFrom(resourceDescriptor.ResourceType));
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_True_If_Type_Is_IIdentfiable()
        {
            // arrange
            var resourceType = typeof(Model);

            // act
            var isJsonApiResource = TypeLocator.TryGetResourceDescriptor(resourceType, out var descriptor);

            // assert
            Assert.True(isJsonApiResource);
            Assert.Equal(resourceType, descriptor.ResourceType);
            Assert.Equal(typeof(int), descriptor.IdType);
        }

        [Fact]
        public void TryGetResourceDescriptor_Returns_False_If_Type_Is_IIdentfiable()
        {
            // arrange
            var resourceType = typeof(String);

            // act
            var isJsonApiResource = TypeLocator.TryGetResourceDescriptor(resourceType, out var _);

            // assert
            Assert.False(isJsonApiResource);
        }
    }

    
    public interface IGenericInterface<T> { }
    public class Implementation : IGenericInterface<int> { }


    public class BaseType<T> { }
    public class DerivedType : BaseType<int> { }

    public class Model : Identifiable { }
}
