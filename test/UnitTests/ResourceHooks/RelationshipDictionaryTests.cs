using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class Dummy : Identifiable
    {
        public string SomeUpdatedProperty { get; set; }
        public string SomeNotUpdatedProperty { get; set; }

        [HasOne]
        public ToOne FirstToOne { get; set; }
        [HasOne]
        public ToOne SecondToOne { get; set; }
        [HasMany]
        public ISet<ToMany> ToManies { get; set; }
    }

    public class NotTargeted : Identifiable { }
    public sealed class ToMany : Identifiable { }
    public sealed class ToOne : Identifiable { }

    public sealed class RelationshipDictionaryTests
    {
        public readonly HasOneAttribute FirstToOneAttr;
        public readonly HasOneAttribute SecondToOneAttr;
        public readonly HasManyAttribute ToManyAttr;

        public readonly Dictionary<RelationshipAttribute, HashSet<Dummy>> Relationships = new Dictionary<RelationshipAttribute, HashSet<Dummy>>();
        public readonly HashSet<Dummy> FirstToOnesResources = new HashSet<Dummy> { new Dummy { Id = 1 }, new Dummy { Id = 2 }, new Dummy { Id = 3 } };
        public readonly HashSet<Dummy> SecondToOnesResources = new HashSet<Dummy> { new Dummy { Id = 4 }, new Dummy { Id = 5 }, new Dummy { Id = 6 } };
        public readonly HashSet<Dummy> ToManiesResources = new HashSet<Dummy> { new Dummy { Id = 7 }, new Dummy { Id = 8 }, new Dummy { Id = 9 } };
        public readonly HashSet<Dummy> NoRelationshipsResources = new HashSet<Dummy> { new Dummy { Id = 10 }, new Dummy { Id = 11 }, new Dummy { Id = 12 } };
        public readonly HashSet<Dummy> AllResources;
        public RelationshipDictionaryTests()
        {
            FirstToOneAttr = new HasOneAttribute
            {
                PublicName = "firstToOne",
                LeftType = typeof(Dummy),
                RightType = typeof(ToOne),
                Property = typeof(Dummy).GetProperty(nameof(Dummy.FirstToOne))
            };
            SecondToOneAttr = new HasOneAttribute
            {
                PublicName = "secondToOne",
                LeftType = typeof(Dummy),
                RightType = typeof(ToOne),
                Property = typeof(Dummy).GetProperty(nameof(Dummy.SecondToOne))
            };
            ToManyAttr = new HasManyAttribute
            {
                PublicName = "toManies",
                LeftType = typeof(Dummy),
                RightType = typeof(ToMany),
                Property = typeof(Dummy).GetProperty(nameof(Dummy.ToManies))
            };
            Relationships.Add(FirstToOneAttr, FirstToOnesResources);
            Relationships.Add(SecondToOneAttr, SecondToOnesResources);
            Relationships.Add(ToManyAttr, ToManiesResources);
            AllResources = new HashSet<Dummy>(FirstToOnesResources.Union(SecondToOnesResources).Union(ToManiesResources).Union(NoRelationshipsResources));
        }

        [Fact]
        public void RelationshipsDictionary_GetByRelationships()
        {
            // Arrange
            RelationshipsDictionary<Dummy> relationshipsDictionary = new RelationshipsDictionary<Dummy>(Relationships);

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
            RelationshipsDictionary<Dummy> relationshipsDictionary = new RelationshipsDictionary<Dummy>(Relationships);

            // Act
            var affectedThroughFirstToOne = relationshipsDictionary.GetAffected(d => d.FirstToOne).ToList();
            var affectedThroughSecondToOne = relationshipsDictionary.GetAffected(d => d.SecondToOne).ToList();
            var affectedThroughToMany = relationshipsDictionary.GetAffected(d => d.ToManies).ToList();

            // Assert
            affectedThroughFirstToOne.ForEach(resource => Assert.Contains(resource, FirstToOnesResources));
            affectedThroughSecondToOne.ForEach(resource => Assert.Contains(resource, SecondToOnesResources));
            affectedThroughToMany.ForEach(resource => Assert.Contains(resource, ToManiesResources));
        }

        [Fact]
        public void ResourceHashSet_GetByRelationships()
        {
            // Arrange
            ResourceHashSet<Dummy> resources = new ResourceHashSet<Dummy>(AllResources, Relationships);

            // Act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = resources.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = resources.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = resources.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = resources.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            var allResourcesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
            NoRelationshipsResources.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allResourcesWithAffectedRelationships);
            });
        }

        [Fact]
        public void ResourceDiff_GetByRelationships()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(AllResources.Select(e => new Dummy { Id = e.Id }).ToList());
            DiffableResourceHashSet<Dummy> diffs = new DiffableResourceHashSet<Dummy>(AllResources, dbResources, Relationships, null);

            // Act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = diffs.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = diffs.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = diffs.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = diffs.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            var allResourcesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
            NoRelationshipsResources.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allResourcesWithAffectedRelationships);
            });

            var requestResourcesFromDiff = diffs;
            requestResourcesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, AllResources);
            });
            var databaseResourcesFromDiff = diffs.GetDiffs().Select(d => d.DatabaseValue);
            databaseResourcesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, dbResources);
            });
        }

        [Fact]
        public void ResourceDiff_Loops_Over_Diffs()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(AllResources.Select(e => new Dummy { Id = e.Id }));
            DiffableResourceHashSet<Dummy> diffs = new DiffableResourceHashSet<Dummy>(AllResources, dbResources, Relationships, null);

            // Assert & act
            foreach (ResourceDiffPair<Dummy> diff in diffs.GetDiffs())
            {
                Assert.Equal(diff.Resource.Id, diff.DatabaseValue.Id);
                Assert.NotEqual(diff.Resource, diff.DatabaseValue);
                Assert.Contains(diff.Resource, AllResources);
                Assert.Contains(diff.DatabaseValue, dbResources);
            }
        }

        [Fact]
        public void ResourceDiff_GetAffected_Relationships()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(AllResources.Select(e => new Dummy { Id = e.Id }));
            DiffableResourceHashSet<Dummy> diffs = new DiffableResourceHashSet<Dummy>(AllResources, dbResources, Relationships, null);

            // Act
            var affectedThroughFirstToOne = diffs.GetAffected(d => d.FirstToOne).ToList();
            var affectedThroughSecondToOne = diffs.GetAffected(d => d.SecondToOne).ToList();
            var affectedThroughToMany = diffs.GetAffected(d => d.ToManies).ToList();

            // Assert
            affectedThroughFirstToOne.ForEach(resource => Assert.Contains(resource, FirstToOnesResources));
            affectedThroughSecondToOne.ForEach(resource => Assert.Contains(resource, SecondToOnesResources));
            affectedThroughToMany.ForEach(resource => Assert.Contains(resource, ToManiesResources));
        }

        [Fact]
        public void ResourceDiff_GetAffected_Attributes()
        {
            // Arrange
            var dbResources = new HashSet<Dummy>(AllResources.Select(e => new Dummy { Id = e.Id }));
            var updatedAttributes = new Dictionary<PropertyInfo, HashSet<Dummy>>
            {
                { typeof(Dummy).GetProperty("SomeUpdatedProperty"), AllResources }
            };
            DiffableResourceHashSet<Dummy> diffs = new DiffableResourceHashSet<Dummy>(AllResources, dbResources, Relationships, updatedAttributes);

            // Act
            var affectedThroughSomeUpdatedProperty = diffs.GetAffected(d => d.SomeUpdatedProperty);
            var affectedThroughSomeNotUpdatedProperty = diffs.GetAffected(d => d.SomeNotUpdatedProperty);

            // Assert
            Assert.NotEmpty(affectedThroughSomeUpdatedProperty);
            Assert.Empty(affectedThroughSomeNotUpdatedProperty);
        }

        private void AssertRelationshipDictionaryGetters(Dictionary<RelationshipAttribute, HashSet<Dummy>> relationshipsDictionary,
        Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes,
        Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies,
        Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted)
        {
            Assert.Contains(FirstToOneAttr, toOnes.Keys);
            Assert.Contains(SecondToOneAttr, toOnes.Keys);
            Assert.Contains(ToManyAttr, toManies.Keys);
            Assert.Equal(relationshipsDictionary.Keys.Count, toOnes.Keys.Count + toManies.Keys.Count + notTargeted.Keys.Count);

            toOnes[FirstToOneAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, FirstToOnesResources);
            });

            toOnes[SecondToOneAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, SecondToOnesResources);
            });

            toManies[ToManyAttr].ToList().ForEach(resource =>
            {
                Assert.Contains(resource, ToManiesResources);
            });
            Assert.Empty(notTargeted);
        }
    }
}
