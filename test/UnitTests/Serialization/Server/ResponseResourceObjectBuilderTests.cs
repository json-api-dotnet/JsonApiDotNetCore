using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Server
{
    public sealed class ResponseResourceObjectBuilderTests : SerializerTestsSetup
    {
        private const string RelationshipName = "dependents";
        private readonly List<RelationshipAttribute> _relationshipsForBuild;

        public ResponseResourceObjectBuilderTests()
        {
            _relationshipsForBuild = ResourceGraph.GetRelationships<OneToManyPrincipal>(relationship => new
            {
                relationship.Dependents
            }).ToList();
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksEnabled_RelationshipObjectWithLinks()
        {
            // Arrange
            var resource = new OneToManyPrincipal
            {
                Id = 10
            };

            ResponseResourceObjectBuilder builder = GetResponseResourceObjectBuilder(relationshipLinks: DummyRelationshipLinks);

            // Act
            ResourceObject resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out RelationshipObject relationshipObject));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", relationshipObject.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", relationshipObject.Links.Related);
            Assert.False(relationshipObject.Data.IsPopulated);
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksDisabled_NoRelationshipObject()
        {
            // Arrange
            var resource = new OneToManyPrincipal
            {
                Id = 10
            };

            ResponseResourceObjectBuilder builder = GetResponseResourceObjectBuilder();

            // Act
            ResourceObject resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.Null(resourceObject.Relationships);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksDisabled_RelationshipObjectWithData()
        {
            // Arrange
            var resource = new OneToManyPrincipal
            {
                Id = 10,
                Dependents = new HashSet<OneToManyDependent>
                {
                    new()
                    {
                        Id = 20
                    }
                }
            };

            ResponseResourceObjectBuilder builder = GetResponseResourceObjectBuilder(_relationshipsForBuild.AsEnumerable());

            // Act
            ResourceObject resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out RelationshipObject relationshipObject));
            Assert.Null(relationshipObject.Links);
            Assert.True(relationshipObject.Data.IsPopulated);
            Assert.Equal("20", relationshipObject.ManyData.Single().Id);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksEnabled_RelationshipObjectWithDataAndLinks()
        {
            // Arrange
            var resource = new OneToManyPrincipal
            {
                Id = 10,
                Dependents = new HashSet<OneToManyDependent>
                {
                    new()
                    {
                        Id = 20
                    }
                }
            };

            ResponseResourceObjectBuilder builder =
                GetResponseResourceObjectBuilder(_relationshipsForBuild.AsEnumerable(), relationshipLinks: DummyRelationshipLinks);

            // Act
            ResourceObject resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out RelationshipObject relationshipObject));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", relationshipObject.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", relationshipObject.Links.Related);
            Assert.True(relationshipObject.Data.IsPopulated);
            Assert.Equal("20", relationshipObject.ManyData.Single().Id);
        }
    }
}
