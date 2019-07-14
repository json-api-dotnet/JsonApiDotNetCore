using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Reflection;

namespace UnitTests.ResourceHooks.AffectedEntities
{
    public class Dummy : Identifiable
    {
        public string SomeUpdatedProperty { get; set; }
        public string SomeNotUpdatedProperty { get; set; }


        [HasOne("first-to-one")]
        public ToOne FirstToOne { get; set; }
        [HasOne("second-to-one")]
        public ToOne SecondToOne { get; set; }
        [HasMany("to-manies")]
        public List<ToMany> ToManies { get; set; }
    }
    public class NotTargeted : Identifiable { }
    public class ToMany : Identifiable { }
    public class ToOne : Identifiable { }

    public class RelationshipDictionaryTests
    {
        public readonly HasOneAttribute FirstToOneAttr;
        public readonly HasOneAttribute SecondToOneAttr;
        public readonly HasManyAttribute ToManyAttr;

        public readonly Dictionary<RelationshipAttribute, HashSet<Dummy>> Relationships = new Dictionary<RelationshipAttribute, HashSet<Dummy>>();
        public readonly HashSet<Dummy> FirstToOnesEntities = new HashSet<Dummy> { new Dummy() { Id = 1 }, new Dummy() { Id = 2 }, new Dummy() { Id = 3 } };
        public readonly HashSet<Dummy> SecondToOnesEntities = new HashSet<Dummy> { new Dummy() { Id = 4 }, new Dummy() { Id = 5 }, new Dummy() { Id = 6 } };
        public readonly HashSet<Dummy> ToManiesEntities = new HashSet<Dummy> { new Dummy() { Id = 7 }, new Dummy() { Id = 8 }, new Dummy() { Id = 9 } };
        public readonly HashSet<Dummy> NoRelationshipsEntities = new HashSet<Dummy> { new Dummy() { Id = 10 }, new Dummy() { Id = 11 }, new Dummy() { Id = 12 } };
        public readonly HashSet<Dummy> AllEntities;
        public RelationshipDictionaryTests()
        {
            FirstToOneAttr = new HasOneAttribute("first-to-one")
            {
                PrincipalType = typeof(Dummy),
                DependentType = typeof(ToOne),
                InternalRelationshipName = "FirstToOne"
            };
            SecondToOneAttr = new HasOneAttribute("second-to-one")
            {
                PrincipalType = typeof(Dummy),
                DependentType = typeof(ToOne),
                InternalRelationshipName = "SecondToOne"
            };
            ToManyAttr = new HasManyAttribute("to-manies")
            {
                PrincipalType = typeof(Dummy),
                DependentType = typeof(ToMany),
                InternalRelationshipName = "ToManies"
            };
            Relationships.Add(FirstToOneAttr, FirstToOnesEntities);
            Relationships.Add(SecondToOneAttr, SecondToOnesEntities);
            Relationships.Add(ToManyAttr, ToManiesEntities);
            AllEntities = new HashSet<Dummy>(FirstToOnesEntities.Union(SecondToOnesEntities).Union(ToManiesEntities).Union(NoRelationshipsEntities));
        }

        [Fact]
        public void RelationshipsDictionary_GetByRelationships()
        {
            // arrange 
            RelationshipsDictionary<Dummy> relationshipsDictionary = new RelationshipsDictionary<Dummy>(Relationships);

            // act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = relationshipsDictionary.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = relationshipsDictionary.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = relationshipsDictionary.GetByRelationship<NotTargeted>();

            // assert
            AssertRelationshipDictionaryGetters(relationshipsDictionary, toOnes, toManies, notTargeted);
        }

        [Fact]
        public void RelationshipsDictionary_GetAffected()
        {
            // arrange 
            RelationshipsDictionary<Dummy> relationshipsDictionary = new RelationshipsDictionary<Dummy>(Relationships);

            // act
            var affectedThroughFirstToOne = relationshipsDictionary.GetAffected(d => d.FirstToOne).ToList();
            var affectedThroughSecondToOne = relationshipsDictionary.GetAffected(d => d.SecondToOne).ToList();
            var affectedThroughToMany = relationshipsDictionary.GetAffected(d => d.ToManies).ToList();

            // assert
            affectedThroughFirstToOne.ForEach((entitiy) => Assert.Contains(entitiy, FirstToOnesEntities));
            affectedThroughSecondToOne.ForEach((entitiy) => Assert.Contains(entitiy, SecondToOnesEntities));
            affectedThroughToMany.ForEach((entitiy) => Assert.Contains(entitiy, ToManiesEntities));
        }

        [Fact]
        public void EntityHashSet_GetByRelationships()
        {
            // arrange 
            EntityHashSet<Dummy> entities = new EntityHashSet<Dummy>(AllEntities, Relationships);

            // act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = entities.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = entities.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = entities.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = entities.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            var allEntitiesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
            NoRelationshipsEntities.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allEntitiesWithAffectedRelationships);
            });
        }

        [Fact]
        public void EntityDiff_GetByRelationships()
        {
            // arrange 
            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }).ToList());
            DiffableEntityHashSet<Dummy> diffs = new DiffableEntityHashSet<Dummy>(AllEntities, dbEntities, Relationships, null);

            // act
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = diffs.GetByRelationship<ToOne>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = diffs.GetByRelationship<ToMany>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = diffs.GetByRelationship<NotTargeted>();
            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = diffs.AffectedRelationships;

            // Assert
            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
            var allEntitiesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
            NoRelationshipsEntities.ToList().ForEach(e =>
            {
                Assert.DoesNotContain(e, allEntitiesWithAffectedRelationships);
            });

            var requestEntitiesFromDiff = diffs;
            requestEntitiesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, AllEntities);
            });
            var databaseEntitiesFromDiff = diffs.GetDiffs().Select(d => d.DatabaseValue);
            databaseEntitiesFromDiff.ToList().ForEach(e =>
            {
                Assert.Contains(e, dbEntities);
            });
        }

        [Fact]
        public void EntityDiff_Loops_Over_Diffs()
        {
            // arrange 
            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }));
            DiffableEntityHashSet<Dummy> diffs = new DiffableEntityHashSet<Dummy>(AllEntities, dbEntities, Relationships, null);

            // Assert & act
            foreach (EntityDiffPair<Dummy> diff in diffs.GetDiffs())
            {
                Assert.Equal(diff.Entity.Id, diff.DatabaseValue.Id);
                Assert.NotEqual(diff.Entity, diff.DatabaseValue);
                Assert.Contains(diff.Entity, AllEntities);
                Assert.Contains(diff.DatabaseValue, dbEntities);
            }
        }

        [Fact]
        public void EntityDiff_GetAffected_Relationships()
        {
            // arrange 
            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }));
            DiffableEntityHashSet<Dummy> diffs = new DiffableEntityHashSet<Dummy>(AllEntities, dbEntities, Relationships, null);

            // act
            var affectedThroughFirstToOne = diffs.GetAffected(d => d.FirstToOne).ToList();
            var affectedThroughSecondToOne = diffs.GetAffected(d => d.SecondToOne).ToList();
            var affectedThroughToMany = diffs.GetAffected(d => d.ToManies).ToList();

            // assert
            affectedThroughFirstToOne.ForEach((entitiy) => Assert.Contains(entitiy, FirstToOnesEntities));
            affectedThroughSecondToOne.ForEach((entitiy) => Assert.Contains(entitiy, SecondToOnesEntities));
            affectedThroughToMany.ForEach((entitiy) => Assert.Contains(entitiy, ToManiesEntities));
        }

        [Fact]
        public void EntityDiff_GetAffected_Attributes()
        {
            // arrange 
            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }));
            var updatedAttributes = new Dictionary<PropertyInfo, HashSet<Dummy>>
            {
                { typeof(Dummy).GetProperty("SomeUpdatedProperty"), AllEntities }
            };
            DiffableEntityHashSet<Dummy> diffs = new DiffableEntityHashSet<Dummy>(AllEntities, dbEntities, Relationships, updatedAttributes);

            // act
            var affectedThroughSomeUpdatedProperty = diffs.GetAffected(d => d.SomeUpdatedProperty).ToList();
            var affectedThroughSomeNotUpdatedProperty = diffs.GetAffected(d => d.SomeNotUpdatedProperty).ToList();

            // assert
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

            toOnes[FirstToOneAttr].ToList().ForEach((entitiy) =>
            {
                Assert.Contains(entitiy, FirstToOnesEntities);
            });

            toOnes[SecondToOneAttr].ToList().ForEach((entity) =>
            {
                Assert.Contains(entity, SecondToOnesEntities);
            });

            toManies[ToManyAttr].ToList().ForEach((entitiy) =>
            {
                Assert.Contains(entitiy, ToManiesEntities);
            });
            Assert.Empty(notTargeted);
        }
    }
}
