using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using Xunit;
using UnitTests.TestModels;

namespace UnitTests.Serialization.Serializer
{
    public class ResourceObjectBuilderTests : SerializerTestsSetup
    {
        private readonly ResourceObjectBuilder _builder;

        public ResourceObjectBuilderTests()
        {
            _builder = new ResourceObjectBuilder(_resourceGraph, new ResourceObjectBuilderSettings());
        }

        [Fact]
        public void EntityToResourceObject_EmptyResource_CanBuild()
        {
            // Arrange
            var entity = new TestResource();

            // Act
            var resourceObject = _builder.Build(entity);

            // Assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("testResource", resourceObject.Type);
        }

        [Fact]
        public void EntityToResourceObject_ResourceWithId_CanBuild()
        {
            // Arrange
            var entity = new TestResource { Id = 1 };

            // Act
            var resourceObject = _builder.Build(entity);

            // Assert
            Assert.Equal("1", resourceObject.Id);
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Equal("testResource", resourceObject.Type);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("string field", 1)]
        public void EntityToResourceObject_ResourceWithIncludedAttrs_CanBuild(string stringFieldValue, int? intFieldValue)
        {
            // Arrange
            var entity = new TestResource { StringField = stringFieldValue, NullableIntField = intFieldValue };
            var attrs = _resourceGraph.GetAttributes<TestResource>(tr => new { tr.StringField, tr.NullableIntField });

            // Act
            var resourceObject = _builder.Build(entity, attrs);

            // Assert
            Assert.NotNull(resourceObject.Attributes);
            Assert.Equal(2, resourceObject.Attributes.Keys.Count);
            Assert.Equal(stringFieldValue, resourceObject.Attributes["stringField"]);
            Assert.Equal(intFieldValue, resourceObject.Attributes["nullableIntField"]);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_EmptyResource_CanBuild()
        {
            // Arrange
            var entity = new MultipleRelationshipsPrincipalPart();

            // Act
            var resourceObject = _builder.Build(entity);

            // Assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("multiPrincipals", resourceObject.Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_ResourceWithId_CanBuild()
        {
            // Arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
            };

            // Act
            var resourceObject = _builder.Build(entity);

            // Assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("multiPrincipals", resourceObject.Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_WithIncludedRelationshipsAttributes_CanBuild()
        {
            // Arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var relationships = _resourceGraph.GetRelationships<MultipleRelationshipsPrincipalPart>(tr => new { tr.PopulatedToManies, tr.PopulatedToOne, tr.EmptyToOne, tr.EmptyToManies });

            // Act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // Assert
            Assert.Equal(4, resourceObject.Relationships.Count);
            Assert.Null(resourceObject.Relationships["emptyToOne"].Data);
            Assert.Empty((IList)resourceObject.Relationships["emptyToManies"].Data);
            var populatedToOneData = (ResourceIdentifierObject)resourceObject.Relationships["populatedToOne"].Data;
            Assert.NotNull(populatedToOneData);
            Assert.Equal("10", populatedToOneData.Id);
            Assert.Equal("oneToOneDependents", populatedToOneData.Type);
            var populatedToManiesData = (List<ResourceIdentifierObject>)resourceObject.Relationships["populatedToManies"].Data;
            Assert.Single(populatedToManiesData);
            Assert.Equal("20", populatedToManiesData.First().Id);
            Assert.Equal("oneToManyDependents", populatedToManiesData.First().Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // Arrange
            var entity = new OneToOneDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _resourceGraph.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // Act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // Assert
            Assert.Single(resourceObject.Relationships);
            Assert.NotNull(resourceObject.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)resourceObject.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // Arrange
            var entity = new OneToOneDependent { Principal = null, PrincipalId = 123 };
            var relationships = _resourceGraph.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // Act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // Assert
            Assert.Null(resourceObject.Relationships["principal"].Data);
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // Arrange
            var entity = new OneToOneRequiredDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _resourceGraph.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // Act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // Assert
            Assert.Single(resourceObject.Relationships);
            Assert.NotNull(resourceObject.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)resourceObject.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // Arrange
            var entity = new OneToOneRequiredDependent { Principal = null, PrincipalId = 123 };
            var relationships = _resourceGraph.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // Act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _builder.Build(entity, relationships: relationships));
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_EmptyResourceWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // Arrange
            var entity = new OneToOneRequiredDependent();
            var relationships = _resourceGraph.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // Act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _builder.Build(entity, relationships: relationships));
        }
    }
}
