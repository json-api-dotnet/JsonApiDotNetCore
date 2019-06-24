//using JsonApiDotNetCore.Models;
//using JsonApiDotNetCore.Hooks;
//using System.Collections.Generic;
//using Xunit;
//using System.Linq;

//namespace UnitTests.ResourceHooks.AffectedEntities
//{
//    public class Dummy : Identifiable { }
//    public class NotTargeted : Identifiable { }
//    public class ToMany : Identifiable { }
//    public class ToOne : Identifiable { }

//    public class AffectedEntitiesHelperTests
//    {

//        public readonly HasOneAttribute FirstToOneAttr;
//        public readonly HasOneAttribute SecondToOneAttr;
//        public readonly HasManyAttribute ToManyAttr;


//        public readonly Dictionary<RelationshipAttribute, HashSet<Dummy>> Relationships = new Dictionary<RelationshipAttribute, HashSet<Dummy>>();
//        public readonly HashSet<Dummy> FirstToOnesEntities = new HashSet<Dummy> { new Dummy() { Id = 1 }, new Dummy() { Id = 2 }, new Dummy() { Id = 3 } };
//        public readonly HashSet<Dummy> SecondToOnesEntities = new HashSet<Dummy> { new Dummy() { Id = 4 }, new Dummy() { Id = 5 }, new Dummy() { Id = 6 } };
//        public readonly HashSet<Dummy> ToManiesEntities = new HashSet<Dummy> { new Dummy() { Id = 7 }, new Dummy() { Id = 8 }, new Dummy() { Id = 9 } };
//        public readonly HashSet<Dummy> NoRelationshipsEntities = new HashSet<Dummy> { new Dummy() { Id = 10 }, new Dummy() { Id = 11 }, new Dummy() { Id = 12 } };
//        public readonly HashSet<Dummy> AllEntities;
//        public AffectedEntitiesHelperTests()
//        {
//            FirstToOneAttr = new HasOneAttribute("first-to-one")
//            {
//                PrincipalType = typeof(Dummy),
//                DependentType = typeof(ToOne)
//            };
//            SecondToOneAttr = new HasOneAttribute("second-to-one")
//            {
//                PrincipalType = typeof(Dummy),
//                DependentType = typeof(ToOne)
//            };
//            ToManyAttr = new HasManyAttribute("to-manies")
//            {
//                PrincipalType = typeof(Dummy),
//                DependentType = typeof(ToMany)
//            };
//            Relationships.Add(FirstToOneAttr, FirstToOnesEntities);
//            Relationships.Add(SecondToOneAttr, SecondToOnesEntities);
//            Relationships.Add(ToManyAttr, ToManiesEntities);
//            AllEntities = new HashSet<Dummy>(FirstToOnesEntities.Union(SecondToOnesEntities).Union(ToManiesEntities).Union(NoRelationshipsEntities));
//        }

//        [Fact]
//        public void RelationshipsDictionary_GetByRelationships()
//        {
//            // arrange 
//            RelationshipsDictionary<Dummy> relationshipsDictionary = new RelationshipsDictionary<Dummy>(Relationships);

//            // act
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = relationshipsDictionary.GetByRelationship<ToOne>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = relationshipsDictionary.GetByRelationship<ToMany>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = relationshipsDictionary.GetByRelationship<NotTargeted>();

//            // assert
//            AssertRelationshipDictionaryGetters(relationshipsDictionary, toOnes, toManies, notTargeted);
//        }

//        [Fact]
//        public void EntityHashSet_GetByRelationships()
//        {
//            // arrange 
//            EntityHashSet<Dummy> entities = new EntityHashSet<Dummy>(AllEntities, Relationships);

//            // act
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = entities.GetByRelationship<ToOne>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = entities.GetByRelationship<ToMany>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = entities.GetByRelationship<NotTargeted>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = entities.AffectedRelationships;

//            // Assert
//            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
//            var allEntitiesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
//            NoRelationshipsEntities.ToList().ForEach(e =>
//            {
//                Assert.DoesNotContain(e, allEntitiesWithAffectedRelationships);
//            });
//        }

//        [Fact]
//        public void EntityDiff_GetByRelationships()
//        {
//            // arrange 
//            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }).ToList());
//            EntityDiffs<Dummy> diffs = new EntityDiffs<Dummy>(AllEntities, dbEntities, Relationships);

//            // act
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes = diffs.Entities.GetByRelationship<ToOne>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies = diffs.Entities.GetByRelationship<ToMany>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted = diffs.Entities.GetByRelationship<NotTargeted>();
//            Dictionary<RelationshipAttribute, HashSet<Dummy>> allRelationships = diffs.Entities.AffectedRelationships;

//            // Assert
//            AssertRelationshipDictionaryGetters(allRelationships, toOnes, toManies, notTargeted);
//            var allEntitiesWithAffectedRelationships = allRelationships.SelectMany(kvp => kvp.Value).ToList();
//            NoRelationshipsEntities.ToList().ForEach(e =>
//            {
//                Assert.DoesNotContain(e, allEntitiesWithAffectedRelationships);
//            });

//            var requestEntitiesFromDiff = diffs.Entities;
//            requestEntitiesFromDiff.ToList().ForEach(e =>
//            {
//                Assert.Contains(e, AllEntities);
//            });
//            var databaseEntitiesFromDiff = diffs.DatabaseValues;
//            databaseEntitiesFromDiff.ToList().ForEach(e =>
//            {
//                Assert.Contains(e, dbEntities);
//            });
//        }

//        [Fact]
//        public void EntityDiff_Loops_Over_Diffs()
//        {
//            // arrange 
//            var dbEntities = new HashSet<Dummy>(AllEntities.Select(e => new Dummy { Id = e.Id }));
//            EntityDiffs<Dummy> diffs = new EntityDiffs<Dummy>(AllEntities, dbEntities, Relationships);

//            // Assert & act
//            foreach (EntityDiffPair<Dummy> diff in diffs)
//            {
//                Assert.Equal(diff.Entity.Id, diff.DatabaseValue.Id);
//                Assert.NotEqual(diff.Entity, diff.DatabaseValue);
//                Assert.Contains(diff.Entity, AllEntities);
//                Assert.Contains(diff.DatabaseValue, dbEntities);
//            }
 
//        }

//        private void AssertRelationshipDictionaryGetters(Dictionary<RelationshipAttribute, HashSet<Dummy>> relationshipsDictionary,
//        Dictionary<RelationshipAttribute, HashSet<Dummy>> toOnes,
//        Dictionary<RelationshipAttribute, HashSet<Dummy>> toManies,
//        Dictionary<RelationshipAttribute, HashSet<Dummy>> notTargeted)
//        {
//            Assert.Contains(FirstToOneAttr, toOnes.Keys);
//            Assert.Contains(SecondToOneAttr, toOnes.Keys);
//            Assert.Contains(ToManyAttr, toManies.Keys);
//            Assert.Equal(relationshipsDictionary.Keys.Count, toOnes.Keys.Count + toManies.Keys.Count + notTargeted.Keys.Count);

//            toOnes[FirstToOneAttr].ToList().ForEach((entitiy) =>
//            {
//                Assert.Contains(entitiy, FirstToOnesEntities);
//            });

//            toOnes[SecondToOneAttr].ToList().ForEach((entity) =>
//            {
//                Assert.Contains(entity, SecondToOnesEntities);
//            });

//            toManies[ToManyAttr].ToList().ForEach((entitiy) =>
//            {
//                Assert.Contains(entitiy, ToManiesEntities);
//            });
//            Assert.Empty(notTargeted);
//        }

//    }
//}