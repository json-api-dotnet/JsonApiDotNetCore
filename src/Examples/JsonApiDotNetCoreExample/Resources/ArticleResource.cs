//using System.Collections.Generic;
//using System.Linq;
//using System;
//using JsonApiDotNetCore.Internal;
//using JsonApiDotNetCore.Models;
//using JsonApiDotNetCore.Services;
//using JsonApiDotNetCoreExample.Models;

//namespace JsonApiDotNetCoreExample.Resources
//{
//    public class ArticleResource : ResourceDefinition<Article>
//    {

//        //[LoadDatabaseValues(false)]
//        //public override IEnumerable<Article> BeforeUpdate(EntityDiff<Article> entityDiff, ResourceAction pipeline) // list<string> updated attrs
//        //{
//        //    var entitiesFromRequest = entityDiff.RequestEntities;       // populated met alleen de velden die in de request zaten (attrs en relaties)
//        //    var entitiesFromDatabase = entityDiff.DatabaseEntities;     // null

//        //    return FunkyBusinessLogic(entitiesFromRequest);
//        //}


//        //[LoadDatabaseValues(true)]
//        //public override IEnumerable<Article> BeforeUpdate(EntityDiff<Article> entityDiff, ResourceAction pipeline)
//        //{
//        //    var entitiesFromRequest = entityDiff.RequestEntities;       // populated met alleen de velden die in de request zaten (attrs en relaties)
//        //    var entitiesFromDatabase = entityDiff.DatabaseEntities;     // fully populated: alle velden en ale relaties

//        //    return FunkyBusinessLogicMetDiff(entitiesFromRequest, entitiesFromDatabase);
//        //}


//        //[LoadDatabaseValues(false)]
//        //public override IEnumerable<Article> BeforeUpdateRelationship(IEnumerable<Article> entities, ResourceAction pipeline, IUpdatedRelationshipHelper<Article> relationshipHelper)
//        //{
//        //    // wordt niet afgevuurd
//        //}

//        //[LoadDatabaseValues(true)]
//        //public override IEnumerable<Article> BeforeUpdateRelationship(IEnumerable<Article> entities, ResourceAction pipeline, IUpdatedRelationshipHelper<Article> relationshipHelper)
//        //{
//        //    bool any = entities.Any(); // FALSE


//        //    /// geeft een Dictionary 
//        //    /// {
//        //    ///      (1:1) => Article_3 uit db
//        //    ///      (1:N) => Article_4 ut db
//        //    /// }
//        //    relationshipHelper.ImplicitUpdates; 

//        //}
//    }

//    /// <summary>
//    /// long goal: p3 is zowel owner en reviewer, niet toelaten als owner, wel toelaten als reviewer: hoe dit supporten?
//    /// </summary>
//    public class PersonResource : ResourceDefinition<Person>
//    {
//        [LoadDatabaseValues(false)]
//        public override IEnumerable<Person> BeforeUpdateRelationship(HashSet<Person> entities, 
//            ResourceAction pipeline, 
//            IUpdatedRelationshipHelper<Person> relationshipHelper)
//        {

//            // entities: de "lege" objecten (alleen ID gevuld en navigation property terug naar Article, ook wel genaamd in JADNC jargon: Resource Identifier Objects)
//            /// geeft een Dictionary 
//            /// {
//            ///      OwnerRelationAttribute => [ Owner_3 ]  "leeg object; alleen id en navigation property gevuld"
//            ///      ReviewerRelationAttribute => [ Owner_4 ] "leeg object; alleen id en navigation property gevuld"
//            /// }
//            /// 


//            relationshipHelper.GetEntitiesRelatedWith<Article>();

//            return FunkyBusinessLogic(entities);
//        }



//        [LoadDatabaseValues(true)]
//        public override IEnumerable<Person> BeforeUpdateRelationship(IEnumerable<Person> entities, ResourceAction pipeline, IUpdatedRelationshipHelper<Person> relationshipHelper)
//        {

//            // entities: de "lege" objecten (alleen ID gevuld en navigation property terug naar Article, ook wel genaamd in JADNC jargon: Resource Identifier Objects)

//            /// geeft een Dictionary 
//            /// {
//            ///      (1:1) => Owner_3 uit db 
//            ///      (1:N) => Owner_4 uit db
//            /// }
//            relationshipHelper.GetEntitiesRelatedWith<Article>();


//            /// geeft een Dictionary 
//            /// {
//            ///      (1:1) => Owner_1 uit db    
//            ///      (1:N) => Owner_2 uit db
//            /// }
//            var implicitlyAffected = relationshipHelper.ImplicitUpdates;

//            return FunkyBusinessLogic(entities);  // return list<int>!!!!,   entities p3 en p4 relationship helper.
//        }
//    }
//}





////public override IEnumerable<Article> AfterRead(IEnumerable<Article> entities, ResourceAction pipeline, bool nestedHook = false)
////{
////    if (pipeline == ResourceAction.GetSingle && entities.Single().Name == "Classified")
////    {
////        throw new JsonApiException(403, "You are not allowed to see this article!", new UnauthorizedAccessException());
////    }
////    return entities.Where(t => t.Name != "This should be not be included");
////}