using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using Xunit;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization.Serializer
{
    public class ResourceObjectBuilderTests : SerializerTestsSetup
    {
        private readonly ResourceObjectBuilder _builder;

        public ResourceObjectBuilderTests()
        {
            _builder = new ResourceObjectBuilder(_resourceGraph, _resourceGraph, new ResourceObjectBuilderSettings());
        }

        [Fact]
        public void EntityToResourceObject_EmptyResource_CanBuild()
        {
            // arrange
            var entity = new TestResource();

            // act
            var resourceObject = _builder.Build(entity);

            // assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("test-resource", resourceObject.Type);
        }

        [Fact]
        public void EntityToResourceObject_ResourceWithId_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1 };

            // act
            var resourceObject = _builder.Build(entity);

            // assert
            Assert.Equal("1", resourceObject.Id);
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Equal("test-resource", resourceObject.Type);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("string field", 1)]
        public void EntityToResourceObject_ResourceWithIncludedAttrs_CanBuild(string stringFieldValue, int? intFieldValue)
        {
            // arrange
            var entity = new TestResource() { StringField = stringFieldValue, NullableIntField = intFieldValue };
            var attrs = _fieldExplorer.GetAttributes<TestResource>(tr => new { tr.StringField, tr.NullableIntField });

            // act
            var resourceObject = _builder.Build(entity, attrs);

            // assert
            Assert.NotNull(resourceObject.Attributes);
            Assert.Equal(2, resourceObject.Attributes.Keys.Count);
            Assert.Equal(stringFieldValue, resourceObject.Attributes["string-field"]);
            Assert.Equal(intFieldValue, resourceObject.Attributes["nullable-int-field"]);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_EmptyResource_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart();

            // act
            var resourceObject = _builder.Build(entity);

            // assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("multi-principals", resourceObject.Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_ResourceWithId_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
            };

            // act
            var resourceObject = _builder.Build(entity);

            // assert
            Assert.Null(resourceObject.Attributes);
            Assert.Null(resourceObject.Relationships);
            Assert.Null(resourceObject.Id);
            Assert.Equal("multi-principals", resourceObject.Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_WithIncludedRelationshipsAttributes_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var relationships = _fieldExplorer.GetRelationships<MultipleRelationshipsPrincipalPart>(tr => new { tr.PopulatedToManies, tr.PopulatedToOne, tr.EmptyToOne, tr.EmptyToManies });

            // act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // assert
            Assert.Equal(4, resourceObject.Relationships.Count);
            Assert.Null(resourceObject.Relationships["empty-to-one"].Data);
            Assert.Empty((IList)resourceObject.Relationships["empty-to-manies"].Data);
            var populatedToOneData = (ResourceIdentifierObject)resourceObject.Relationships["populated-to-one"].Data;
            Assert.NotNull(populatedToOneData);
            Assert.Equal("10", populatedToOneData.Id);
            Assert.Equal("one-to-one-dependents", populatedToOneData.Type);
            var populatedToManiesData = (List<ResourceIdentifierObject>)resourceObject.Relationships["populated-to-manies"].Data;
            Assert.Equal(1, populatedToManiesData.Count);
            Assert.Equal("20", populatedToManiesData.First().Id);
            Assert.Equal("one-to-many-dependents", populatedToManiesData.First().Type);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // assert
            Assert.Equal(1, resourceObject.Relationships.Count);
            Assert.NotNull(resourceObject.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)resourceObject.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void EntityWithRelationshipsToResourceObject_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneDependent { Principal = null, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // assert
            Assert.Null(resourceObject.Relationships["principal"].Data);
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneRequiredDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act
            var resourceObject = _builder.Build(entity, relationships: relationships);

            // assert
            Assert.Equal(1, resourceObject.Relationships.Count);
            Assert.NotNull(resourceObject.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)resourceObject.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // arrange
            var entity = new OneToOneRequiredDependent { Principal = null, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _builder.Build(entity, relationships: relationships));
        }

        [Fact]
        public void EntityWithRequiredRelationshipsToResourceObject_EmptyResourceWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // arrange
            var entity = new OneToOneRequiredDependent();
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _builder.Build(entity, relationships: relationships));
        }
    }
}
