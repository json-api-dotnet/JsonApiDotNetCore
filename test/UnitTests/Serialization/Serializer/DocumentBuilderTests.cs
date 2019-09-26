using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Serialization.Serializer
{
    public class DocumentBuilderTests : SerializerTestsSetup
    {
        private readonly TestSerializer _serializer;

        public DocumentBuilderTests()
        {
            _serializer = new TestSerializer(_resourceGraph, _resourceGraph);

        }

        [Fact]
        public void ResourceToDocument_EmptyResource_CanBuild()
        {
            // arrange
            var entity = new TestResource();

            // act
            var document = _serializer.Build(entity);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Null(data.Attributes);
            Assert.Null(data.Relationships);
            Assert.Null(data.Id);
            Assert.Equal("test-resource", data.Type);
        }

        [Fact]
        public void ResourceToDocument_ResourceWithId_CanBuild()
        {
            // arrange
            var entity = new TestResource() { Id = 1 };

            // act
            var document = _serializer.Build(entity);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Equal("1", data.Id);
            Assert.Null(data.Attributes);
            Assert.Null(data.Relationships);
            Assert.Equal("test-resource", data.Type);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("string field", 1)]
        public void ResourceToDocument_ResourceWithIncludedAttrs_CanBuild(string stringFieldValue, int? intFieldValue)
        {
            // arrange
            var entity = new TestResource() { StringField = stringFieldValue, NullableIntField = intFieldValue };
            var attrs = _fieldExplorer.GetAttributes<TestResource>(tr => new { tr.StringField, tr.NullableIntField });
            // act
            var document = _serializer.Build(entity, attrs);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.NotNull(data.Attributes);
            Assert.Equal(2, data.Attributes.Keys.Count);
            Assert.Equal(stringFieldValue, data.Attributes["string-field"]);
            Assert.Equal(intFieldValue, data.Attributes["nullable-int-field"]);
        }



        [Theory]
        [InlineData(null, null)]
        [InlineData("string field", 1)]
        public void ResourceListToDocument_ResourcesWithIncludedAttrs_CanBuild(string stringFieldValue, int? intFieldValue)
        {
            // arrange
            var entities = new List<TestResource>()
            {
                new TestResource() { Id = 1, StringField = stringFieldValue, NullableIntField = intFieldValue },
                new TestResource() { Id = 2, StringField = stringFieldValue, NullableIntField = intFieldValue }
            };
            var attrs = _fieldExplorer.GetAttributes<TestResource>(tr => new { tr.StringField, tr.NullableIntField });
            // act
            var document = _serializer.Build(entities, attrs);
            var data = (List<ResourceObject>)document.Data;

            // assert

            Assert.Equal(2, data.Count);
            foreach (var ro in data)
            {
                Assert.Equal(2, ro.Attributes.Keys.Count);
                Assert.Equal(stringFieldValue, ro.Attributes["string-field"]);
                Assert.Equal(intFieldValue, ro.Attributes["nullable-int-field"]);
            }
        }

        [Fact]
        public void ResourceWithRelationshipsToDocument_EmptyResource_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart();

            // act
            var document = _serializer.Build(entity);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Null(data.Attributes);
            Assert.Null(data.Relationships);
            Assert.Null(data.Id);
            Assert.Equal("multi-principals", data.Type);
        }

        [Fact]
        public void ResourceWithRelationshipsToDocument_ResourceWithId_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
            };

            // act
            var document = _serializer.Build(entity);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Null(data.Attributes);
            Assert.Null(data.Relationships);
            Assert.Null(data.Id);
            Assert.Equal("multi-principals", data.Type);
        }

        [Fact]
        public void ResourceWithRelationshipsToDocument_WithIncludedRelationshipsAttributes_CanBuild()
        {
            // arrange
            var entity = new MultipleRelationshipsPrincipalPart
            {
                PopulatedToOne = new OneToOneDependent { Id = 10 },
                PopulatedToManies = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } }
            };
            var relationships = _fieldExplorer.GetRelationships<MultipleRelationshipsPrincipalPart>(tr => new { tr.PopulatedToManies, tr.PopulatedToOne, tr.EmptyToOne, tr.EmptyToManies });

            // act
            var document = _serializer.Build(entity, relationships: relationships);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Equal(4, data.Relationships.Count);
            Assert.Null(data.Relationships["empty-to-one"].Data);
            Assert.Empty((IList)data.Relationships["empty-to-manies"].Data);
            var populatedToOneData = (ResourceIdentifierObject)data.Relationships["populated-to-one"].Data;
            Assert.NotNull(populatedToOneData);
            Assert.Equal("10", populatedToOneData.Id);
            Assert.Equal("one-to-one-dependents", populatedToOneData.Type);
            var populatedToManiesData = (List<ResourceIdentifierObject>)data.Relationships["populated-to-manies"].Data;
            Assert.Equal(1, populatedToManiesData.Count);
            Assert.Equal("20", populatedToManiesData.First().Id);
            Assert.Equal("one-to-many-dependents", populatedToManiesData.First().Type);
        }

        [Fact]
        public void ResourceWithRelationshipsToDocument_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // act
            var document = _serializer.Build(entity, relationships: relationships);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Equal(1, data.Relationships.Count);
            Assert.NotNull(data.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)data.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void ResourceWithRelationshipsToDocument_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneDependent { Principal = null, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneDependent>(tr => tr.Principal);

            // act
            var document = _serializer.Build(entity, relationships: relationships);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Null(data.Relationships["principal"].Data);
        }

        [Fact]
        public void ResourceWithRequiredRelationshipsToDocument_DeviatingForeignKeyWhileRelationshipIncluded_IgnoresForeignKeyDuringBuild()
        {
            // arrange
            var entity = new OneToOneRequiredDependent { Principal = new OneToOnePrincipal { Id = 10 }, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act
            var document = _serializer.Build(entity, relationships: relationships);
            var data = (ResourceObject)document.Data;

            // assert
            Assert.Equal(1, data.Relationships.Count);
            Assert.NotNull(data.Relationships["principal"].Data);
            var ro = (ResourceIdentifierObject)data.Relationships["principal"].Data;
            Assert.Equal("10", ro.Id);
        }

        [Fact]
        public void ResourceWithRequiredRelationshipsToDocument_DeviatingForeignKeyAndNoNavigationWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // arrange
            var entity = new OneToOneRequiredDependent { Principal = null, PrincipalId = 123 };
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _serializer.Build(entity, relationships: relationships));
        }

        [Fact]
        public void ResourceWithRequiredRelationshipsToDocument_EmptyResourceWhileRelationshipIncluded_ThrowsNotSupportedException()
        {
            // arrange
            var entity = new OneToOneRequiredDependent();
            var relationships = _fieldExplorer.GetRelationships<OneToOneRequiredDependent>(tr => tr.Principal);

            // act & assert
            Assert.ThrowsAny<NotSupportedException>(() => _serializer.Build(entity, relationships: relationships));
        }
    }
}
