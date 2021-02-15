using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources.Annotations;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Server
{
    public sealed class ResponseResourceObjectBuilderTests : SerializerTestsSetup
    { 
        private readonly List<RelationshipAttribute> _relationshipsForBuild;
        private const string RelationshipName = "dependents";

        public ResponseResourceObjectBuilderTests()
        {
            _relationshipsForBuild = ResourceGraph.GetRelationships<OneToManyPrincipal>(e => new { e.Dependents }).ToList();
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksEnabled_RelationshipEntryWithLinks()
        {
            // Arrange
            var resource = new OneToManyPrincipal { Id = 10 };
            var builder = GetResponseResourceObjectBuilder(relationshipLinks: _dummyRelationshipLinks);

            // Act
            var resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out var entry));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", entry.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", entry.Links.Related);
            Assert.False(entry.IsPopulated);
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksDisabled_NoRelationshipObject()
        {
            // Arrange
            var resource = new OneToManyPrincipal { Id = 10 };
            var builder = GetResponseResourceObjectBuilder();

            // Act
            var resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.Null(resourceObject.Relationships);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksDisabled_RelationshipEntryWithData()
        {
            // Arrange
            var resource = new OneToManyPrincipal { Id = 10, Dependents = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 20 } } };
            var builder = GetResponseResourceObjectBuilder(inclusionChains: new List<List<RelationshipAttribute>> { _relationshipsForBuild } );

            // Act
            var resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out var entry));
            Assert.Null(entry.Links);
            Assert.True(entry.IsPopulated);
            Assert.Equal("20", entry.ManyData.Single().Id);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksEnabled_RelationshipEntryWithDataAndLinks()
        {
            // Arrange
            var resource = new OneToManyPrincipal { Id = 10, Dependents = new HashSet<OneToManyDependent> { new OneToManyDependent { Id = 20 } } };
            var builder = GetResponseResourceObjectBuilder(inclusionChains: new List<List<RelationshipAttribute>> { _relationshipsForBuild }, relationshipLinks: _dummyRelationshipLinks);

            // Act
            var resourceObject = builder.Build(resource, relationships: _relationshipsForBuild);

            // Assert
            Assert.True(resourceObject.Relationships.TryGetValue(RelationshipName, out var entry));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", entry.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", entry.Links.Related);
            Assert.True(entry.IsPopulated);
            Assert.Equal("20", entry.ManyData.Single().Id);
        }
    }
}
