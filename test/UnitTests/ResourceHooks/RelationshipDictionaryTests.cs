using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class RelationshipDictionaryTests
    {
        private readonly HasOneAttribute _firstToOneAttr;
        private readonly HasOneAttribute _secondToOneAttr;
        private readonly HasManyAttribute _toManyAttr;

        private readonly Dictionary<RelationshipAttribute, HashSet<Dummy>> _relationships = new Dictionary<RelationshipAttribute, HashSet<Dummy>>();
        private readonly HashSet<Dummy> _firstToOnesResources = new HashSet<Dummy> { new Dummy { Id = 1 }, new Dummy { Id = 2 }, new Dummy { Id = 3 } };
        private readonly HashSet<Dummy> _secondToOnesResources = new HashSet<Dummy> { new Dummy { Id = 4 }, new Dummy { Id = 5 }, new Dummy { Id = 6 } };
        private readonly HashSet<Dummy> _toManiesResources = new HashSet<Dummy> { new Dummy { Id = 7 }, new Dummy { Id = 8 }, new Dummy { Id = 9 } };
        private readonly HashSet<Dummy> _noRelationshipsResources = new HashSet<Dummy> { new Dummy { Id = 10 }, new Dummy { Id = 11 }, new Dummy { Id = 12 } };
        private readonly HashSet<Dummy> _allResources;

        public RelationshipDictionaryTests()
        {
            _firstToOneAttr = new HasOneAttribute
            {
                PublicName = "firstToOne",
                LeftType = typeof(Dummy),
                RightType = typeof(ToOne),
                Property = typeof(Dummy).GetProperty(nameof(Dummy.FirstToOne))
            };

            _secondToOneAttr = new HasOneAttribute
            {
                PublicName = "secondToOne",
                LeftType = typeof(Dummy),
                RightType = typeof(ToOne),
                Property = typeof(Dummy).GetProperty(nameof(Dummy.SecondToOne))
            };

            _toManyAttr = new HasManyAttribute
            {
                PublicName = "toManies", LeftType = typeof(Dummy), RightType = typeof(ToMany), Property = typeof(Dummy).GetProperty(nameof(Dummy.ToManies))
            };

            _relationships.Add(_firstToOneAttr, _firstToOnesResources);
            _relationships.Add(_secondToOneAttr, _secondToOnesResources);
            _relationships.Add(_toManyAttr, _toManiesResources);
            _allResources = new HashSet<Dummy>(_firstToOnesResources.Union(_secondToOnesResources).Union(_toManiesResources).Union(_noRelationshipsResources));
        }

        [Fact]
        public void RelationshipsDictionary_GetByRelationships()
        {
            // Arrange
            var relationshipsDictionary = new RelationshipsDictionary<Dummy>(_relationships);

            // Act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = relationshipsDictionary.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = relationshipsDictionary.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = relationshipsDictionary.GetByRelationship<NotTargeted>();

            // Assert
            AssertRelationshipDictionaryGetters(relationshipsDictionary, toOnes, toManies, notTargeted);
        }

        [Fact]
        public void RelationshipsDictionary_GetAffected()
        {
            // Arrange
            var relationshipsDictionary = new RelationshipsDictionary<Dummy>(_relationships);

            // Act
            List<Dummy> affectedThroughFirstToOne = relationshipsDictionary.GetAffected(d => d.FirstToOne).ToList();
            List<Dummy> affectedThroughSecondToOne = relationshipsDictionary.GetAffected(d => d.SecondToOne).ToList();
            List<Dummy> affectedThroughToMany = relationshipsDictionary.GetAffected(d => d.ToManies).ToList();

            // Assert
            affectedThroughFirstToOne.ForEach(resource => Assert.Contains(resource, _firstToOnesResources));
            affectedThroughSecondToOne.ForEach(resource => Assert.Contains(resource, _secondToOnesResources));
            affectedThroughToMany.ForEach(resource => Assert.Contains(resource, _toManiesResources));
        }

        [Fact]
        public void ResourceHashSet_GetByRelationships()
        {
            // Arrange
            var resources = new ResourceHashSet<Dummy>(_allResources, _relationships);

            // Act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = resources.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = resources.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = resources.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = resources.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            List<Dummy> allResourcesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();

            _noRelationshipsResources.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allResourcesWithAffectedRelationships);
            });
        }

        [Fact]
        public void ResourceDiff_GetByRelationships()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(_allResources.Select(e => new Dummy { Id = e.Id }).ToList());
            var diffs = new DiffableResourceHashSet<Dummy>(_allResources, dbResources, _relationships, null);

            // Act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = diffs.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = diffs.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = diffs.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = diffs.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            List<Dummy> allResourcesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();

            _noRelationshipsResources.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allResourcesWithAffectedRelationships);
            });

            DiffableResourceHashSet<Dummy> requestResourcesFromDiff = diffs;

            requestResourcesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, _allResources);
            });

            IEnumerable<Dummy> databaseResourcesFromDiff = diffs.GetDiffs().Select(d => d.DatabaseValue);

            databaseResourcesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, dbResources);
            });
        }

        [Fact]
        public void ResourceDiff_Loops_Over_Diffs()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(_allResources.Select(e => new Dummy { Id = e.Id }));
            var diffs = new DiffableResourceHashSet<Dummy>(_allResources, dbResources, _relationships, null);

            // Assert & act
            foreach (ResourceDiffPair<Dummy> diff in diffs.GetDiffs())
            {
                Assert.Equal(diff.Resource.Id, diff.DatabaseValue.Id);
                Assert.NotEqual(diff.Resource, diff.DatabaseValue);
                Assert.Contains(diff.Resource, _allResources);
                Assert.Contains(diff.DatabaseValue, dbResources);
            }
        }

        [Fact]
        public void ResourceDiff_GetAffected_Relationships()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(_allResources.Select(e => new Dummy { Id = e.Id }));
            var diffs = new DiffableResourceHashSet<Dummy>(_allResources, dbResources, _relationships, null);

            // Act
            List<Dummy> affectedThroughFirstToOne = diffs.GetAffected(d => d.FirstToOne).ToList();
            List<Dummy> affectedThroughSecondToOne = diffs.GetAffected(d => d.SecondToOne).ToList();
            List<Dummy> affectedThroughToMany = diffs.GetAffected(d => d.ToManies).ToList();

            // Assert
            affectedThroughFirstToOne.ForEach(resource => Assert.Contains(resource, _firstToOnesResources));
            affectedThroughSecondToOne.ForEach(resource => Assert.Contains(resource, _secondToOnesResources));
            affectedThroughToMany.ForEach(resource => Assert.Contains(resource, _toManiesResources));
        }

        [Fact]
        public void ResourceDiff_GetAffected_Attributes()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(_allResources.Select(e => new Dummy { Id = e.Id }));

            var updatedAttributes = new Dictionary<PropertyInfo, HashSet<Dummy>>
            {
                { typeof(Dummy).GetProperty(nameof(Dummy.SomeUpdatedProperty))!, _allResources }
            };

            var diffs = new DiffableResourceHashSet<Dummy>(_allResources, dbResources, _relationships, updatedAttributes);

            // Act
            HashSet<Dummy> affectedThroughSomeUpdatedProperty = diffs.GetAffected(d => d.SomeUpdatedProperty);
            HashSet<Dummy> affectedThroughSomeNotUpdatedProperty = diffs.GetAffected(d => d.SomeNotUpdatedProperty);

            // Assert
            Assert.NotEmpty(affectedThroughSomeUpdatedProperty);
            Assert.Empty(affectedThroughSomeNotUpdatedProperty);
        }

        [AssertionMethod]
        private void AssertRelationshipDictionaryGetters(Dictionary<RelationshipAttribute, HashSet<Dummy>> relationshipsDictionary,
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes, Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies,
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted)
        {
            Assert.Contains(_firstToOneAttr, toOnes.Keys);
            Assert.Contains(_secondToOneAttr, toOnes.Keys);
            Assert.Contains(_toManyAttr, toManies.Keys);
            Assert.Equal(relationshipsDictionary.Keys.Count, toOnes.Keys.Count + toManies.Keys.Count + notTargeted.Keys.Count);

            toOnes[_firstToOneAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, _firstToOnesResources);
            });

            toOnes[_secondToOneAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, _secondToOnesResources);
            });

            toManies[_toManyAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, _toManiesResources);
            });

            Assert.Empty(notTargeted);
        }
    }
}
