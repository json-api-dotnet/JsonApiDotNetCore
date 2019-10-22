using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using Xunit;
using UnitTests.TestModels;
using Person = UnitTests.TestModels.Person;

namespace UnitTests.Serialization.Server
{
    public class ResponseResourceObjectBuilderTests : SerializerTestsSetup
    { 
        private readonly List<RelationshipAttribute> _relationshipsForBuild;
        private const string _relationshipName = "dependents";

        public ResponseResourceObjectBuilderTests()
        {
            _relationshipsForBuild = _resourceGraph.GetRelationships<OneToManyPrincipal>(e => new { e.Dependents });
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksEnabled_RelationshipEntryWithLinks()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10 };
            var builder = GetResponseResourceObjectBuilder(relationshipLinks: _dummyRelationshipLinks);

            // act
            var resourceObject = builder.Build(entity, relationships: _relationshipsForBuild);

            // assert
            Assert.True(resourceObject.Relationships.TryGetValue(_relationshipName, out var entry));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", entry.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", entry.Links.Related);
            Assert.False(entry.IsPopulated);
        }

        [Fact]
        public void Build_RelationshipNotIncludedAndLinksDisabled_NoRelationshipObject()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10 };
            var builder = GetResponseResourceObjectBuilder();

            // act
            var resourceObject = builder.Build(entity, relationships: _relationshipsForBuild);

            // assert
            Assert.Null(resourceObject.Relationships);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksDisabled_RelationshipEntryWithData()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10, Dependents = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } } };
            var builder = GetResponseResourceObjectBuilder(inclusionChains: new List<List<RelationshipAttribute>> { _relationshipsForBuild } );

            // act
            var resourceObject = builder.Build(entity, relationships: _relationshipsForBuild);

            // assert
            Assert.True(resourceObject.Relationships.TryGetValue(_relationshipName, out var entry));
            Assert.Null(entry.Links);
            Assert.True(entry.IsPopulated);
            Assert.Equal("20", entry.ManyData.Single().Id);
        }

        [Fact]
        public void Build_RelationshipIncludedAndLinksEnabled_RelationshipEntryWithDataAndLinks()
        {
            // arrange
            var entity = new OneToManyPrincipal { Id = 10, Dependents = new List<OneToManyDependent> { new OneToManyDependent { Id = 20 } } };
            var builder = GetResponseResourceObjectBuilder(inclusionChains: new List<List<RelationshipAttribute>> { _relationshipsForBuild }, relationshipLinks: _dummyRelationshipLinks);

            // act
            var resourceObject = builder.Build(entity, relationships: _relationshipsForBuild);

            // assert
            Assert.True(resourceObject.Relationships.TryGetValue(_relationshipName, out var entry));
            Assert.Equal("http://www.dummy.com/dummy-relationship-self-link", entry.Links.Self);
            Assert.Equal("http://www.dummy.com/dummy-relationship-related-link", entry.Links.Related);
            Assert.True(entry.IsPopulated);
            Assert.Equal("20", entry.ManyData.Single().Id);
        }
    }
}
