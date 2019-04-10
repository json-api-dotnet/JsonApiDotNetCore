using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{


    /// <inheritdoc/>
    public class ResourceHookExecutor<TEntity> : IResourceHookExecutor<TEntity> where TEntity : class, IIdentifiable
    {

        protected readonly ResourceHook[] _implementedHooks;
        protected readonly IJsonApiContext _jsonApiContext;
        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly ResourceDefinition<TEntity> _resourceDefinition;
        protected readonly IResourceGraph _graph;
        protected readonly Type _entityType;
        protected Dictionary<string, Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>> _meta;
        private ResourceHook _hookInTreeTraversal;

        public ResourceHookExecutor(IJsonApiContext jsonApiContext, IImplementedResourceHooks<TEntity> hooksConfiguration)
        {
            _genericProcessorFactory = jsonApiContext.GenericProcessorFactory;
            _jsonApiContext = jsonApiContext;
            _implementedHooks = hooksConfiguration.ImplementedHooks;
            _graph = _jsonApiContext.ResourceGraph;
            _entityType = typeof(TEntity);
            _meta = new Dictionary<string, Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>>();
        }

        /// <inheritdoc/>
        public virtual bool ShouldExecuteHook(ResourceHook hook)
        {
            return _implementedHooks.Contains(hook);
        }

        public virtual TEntity BeforeCreate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.BeforeCreate)) return entity;
            var hookContainer = GetResourceDefinition(_entityType);
            /// traversing the 0th layer. Not including this in the recursive function
            /// because the most complexities that arrise in the tree traversal do not
            /// apply to the 0th layer (eg non-homogeneity of the next layers)
            entity = hookContainer.BeforeCreate(entity, actionSource); // eg all of type {Article}

            /// We use IIdentifiable instead of TEntity, because deeper layers
            /// in the tree traversal will not necessarily be homogenous (i.e. 
            /// not all elements will be some same type T).
            /// eg: this list will be all of type {Article}, but deeper layers 
            /// could consist of { Tag, Author, Comment }
            var currentLayer = new List<IIdentifiable>() { entity };

            /// gets the dictionary containing all (type) info we need for the 
            /// traversal of the next layer. We pass it ResourceHook.BeforeUpdate because
            /// thats the hook we will be calling for the affected relations (see implementation overview).
            var contextEntity = _graph.GetContextEntity(_entityType);
            var meta = CreateOrUpdateMeta(currentLayer, ResourceHook.BeforeUpdate);

            BreadthFirstTraverseLayers(currentLayer, meta, (relatedEntities, correspondingContainer) =>
            {
                // WILL BE AS SIMPLE AS:
                // return correspondingContainer.BeforeUpdate(relatedEntities, actionSource);

                // BECAUSE OF TEntity vs IEnumerable<TEntity> discrepancies (due to one-to-one, one-to-many
                var kak = relatedEntities.First();
                var poep = correspondingContainer.BeforeUpdate(kak, actionSource);
                return new List<IIdentifiable>() { poep }.AsEnumerable();
            });

            return entity;
        }




        /// <inheritdoc/>
        public virtual TEntity AfterCreate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterCreate)) return entity;
            var hookContainer = GetResourceDefinition(_entityType);
            return hookContainer.AfterCreate(entity, actionSource);
        }

        /// <inheritdoc/>
        public virtual void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            if (!ShouldExecuteHook(ResourceHook.BeforeRead)) return;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TEntity> AfterRead(IEnumerable<TEntity> entities, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterRead)) return entities;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual TEntity BeforeUpdate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.BeforeUpdate)) return entity;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual TEntity AfterUpdate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterUpdate)) return entity;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void BeforeDelete(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.BeforeDelete)) return;
            var hookContainer = GetResourceDefinition(_entityType);
            hookContainer.BeforeDelete(entity, actionSource);
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(TEntity entity, bool succeeded, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterDelete)) return;
            var hookContainer = GetResourceDefinition(_entityType);
            hookContainer.AfterDelete(entity, succeeded, actionSource);
        }

        /// <summary>
        /// PSUEDO CODE:
        /// 
        /// Get all property infos where attribute statisfies isAssignable(RelationshipAttribute);
        ///       using JADNC resourcegraph:
        ///       list relatedentitytypes = getrelations(_entityType)
        ///                                     .filter( WhereIsPopulated() )  (?)
        ///                                     .filter( WhereHookForThatEntityIsImplemented() )
        /// ENTITYHOOKS = Get-all-hook-implementations-for(relatedentitytypes) 
        /// 
        /// ^^^^^  MakeDict{TRelationType, IHookExecutor{TRelationType}
        /// 
        /// nextLayer = [];
        /// for (currEntity in rootEntities);
        /// {    
        ///     currEntity.RelationA, currEntity.relation.B, currEntity.relationC 
        ///              transform to 
        ///     relations = [relationA, relationB, relationC]   (relation_i where i = {A, B, C}
        ///     for (i in {A, B, C} )
        ///     {
        ///         adjustedRelation_i = ENTITYHOOKS.hookForRelation_i(relation_i)
        ///         currentEntity.relation_i = adjustedRelation_i
        ///         if (filteredRelation_i != null) nextLayer.push relation_i
        ///     }        
        ///     nextLayerMeta =  MakeDict{typeof(TRelationType), IHookExecutor{TRelationType} 
        /// }
        /// 
        /// Traverse(nextLayer, nextLayerMeta)
        /// </summary>
        void BreadthFirstTraverseLayers(
            IEnumerable<IIdentifiable> currentLayer,
            Dictionary<string, Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>> meta,
            Func<IEnumerable<IIdentifiable>, IResourceHookContainer<IIdentifiable>, IEnumerable<IIdentifiable>> hookExecution
            )
        {
            var nextLayer = new List<IIdentifiable>();
            foreach (IIdentifiable currentLayerEntity in currentLayer)
            {
                foreach (string metaKey in meta.Keys)
                {
                    (var attr, var hookContainer) = meta[metaKey];
                    /// because currentLayer is not type-homogeneous (which is 
                    /// why we need to use IIdentifiable for the list type of 
                    /// that layer), we need to check if relatedType is really 
                    /// related to parentType. We do this through comparison of Metakey
                    string requiredMetaKey = CreateMetaKey(attr.Type, currentLayerEntity.GetType());
                    if (metaKey != requiredMetaKey) continue;
                    var relatedEntities = attr.GetValue(currentLayerEntity);
                    if (!(relatedEntities is IEnumerable<IIdentifiable>))
                    {
                        // actually need to create list instead of casting here
                        // this will break on runtime, but will work for precompilation checks (just put it like this for quick development for now)
                        relatedEntities = relatedEntities as IEnumerable<IIdentifiable>;
                    }
                    relatedEntities = hookExecution(relatedEntities as IEnumerable<IIdentifiable>, hookContainer);
                    attr.SetValue(currentLayerEntity, relatedEntities);

                    // @TODO distinguish between collections (check for length) 
                    // and to one relations (check for null).
                    if (relatedEntities != null) nextLayer.Concat(relatedEntities as IEnumerable<IIdentifiable>);
                }
            }

            // consider making a hard killswitch based on depth number
            if (nextLayer.Count > 0)
            {
                /// there might be new relation types, so we need to check for that
                /// and update our metadict accordingly.
                var updatedMeta = CreateOrUpdateMeta(nextLayer);
                BreadthFirstTraverseLayers(nextLayer, meta, hookExecution);
            }
        }

        /// <summary>
        /// Creates a (helper) dictionary containing meta information needed for
        /// the traversal of the next layer. It contains as 
        ///     keys:   Type, namely typeof(TRelatedType) that will occur in the traversal 
        ///             of the next layer,
        ///     values: a Tuple of 
        ///                * RelationshipAttribute (that contains getters and setters)
        ///                * IResourceHookExecutor{TRelatedType} to access the actual (nested) hook
        /// </summary>
        /// <returns>The meta dict.</returns>
        /// <param name="nextLayer">List of entities of in the current layer</param>
        /// <param name="hook">The target resource hook type</param>
        Dictionary<string, Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>>
            CreateOrUpdateMeta(
            IEnumerable<IIdentifiable> nextLayer,
            ResourceHook hook = ResourceHook.None)
        {
            var types = GetUniqueConcreteTypes(nextLayer);
            _hookInTreeTraversal = _hookInTreeTraversal !=
                                        ResourceHook.None ?
                                        _hookInTreeTraversal :
                                        hook;
            foreach (Type targetType in types)
            {
                var contextEntity = _graph.GetContextEntity(targetType);
                var metaDictForTargetType = contextEntity.Relationships.ToDictionary(
                                        attr => CreateMetaKey(attr.Type, targetType),
                                        attr => CreateTuple(attr));
                /// keep only the meta info we really need for the traversal of the next layer
                /// also remove duplicates (this is an inefficient implementation of meta cache).
                PruneMetaDictionary(metaDictForTargetType, _hookInTreeTraversal);
                _meta = _meta.Concat(metaDictForTargetType)
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            return _meta;
        }

        /// <summary>
        /// Gets a unique list of all the concrete IIdentifiable types within a layer.
        /// </summary>
        /// <returns>The unique concrete types.</returns>
        /// <param name="layer">Layer.</param>
        Type[] GetUniqueConcreteTypes(IEnumerable<IIdentifiable> layer)
        {
            return new HashSet<Type>(layer.Select(e => e.GetType())).ToArray();
        }

        Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>> CreateTuple(RelationshipAttribute attr)
        {
            var hookContainer = (IResourceHookContainer<IIdentifiable>)GetResourceDefinition(attr.Type);
            return new Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>(attr, hookContainer);
        }

        /// <summary>
        /// Creates the key for the meta dict. The RelationshipAttribute that is
        /// in the value of the meta dict is specific for a particular related type
        /// AS WELL AS parent type. This is reflected by the format of the meta key.
        /// </summary>
        /// <returns>The meta key.</returns>
        /// <param name="relatedType">Related type.</param>
        /// <param name="parentType">Parent type.</param>
        string CreateMetaKey(Type relatedType, Type parentType)
        {
            string newKey = $"{relatedType.Name}-{parentType.Name}";
            if (_meta.ContainsKey(newKey))
            {
                return $"DUPLICATE-{Guid.NewGuid()}";
            }
            return newKey;
        }

        /// <summary>
        /// Gets rid of keys in the meta dict that won't be needed for the next layer.
        /// 
        /// It does so by:
        ///     1)  checking if there was at all a IResourceHookExecutor 
        ///         implemented for this type (ResourceDefinition by default);
        ///     2)  then checking if there is a implementation of the particular
        ///         target hook. 
        /// @TODO part (2) still needs to be implemented:
        ///     => get a hold of IImplementedHooks for that particular type
        ///     => or just hookexecutor for that type and call public ShouldExecute method.
        /// @TODO We need to allow pruning of meta dict using relationship strings,
        ///     which can be relevant if we have eg ?include=.. params that we only care about
        ///     also. This becomes important for performance when we have a model with relation amount 
        ///     n >> 0, and inclusion count i ~ 0. 
        /// 
        ///     investigate: maybe value is null for a not included type instead of an empty list,
        ///     and maybe there is an empty list when a relation is included but there were no records.
        ///     if this is true, we can implement all of it a lot more efficient without using more reflection.
        ///     
        /// </summary>
        /// <param name="meta">the meta dictionary</param>
        void PruneMetaDictionary(
            Dictionary<string, Tuple<RelationshipAttribute, IResourceHookContainer<IIdentifiable>>> meta,
            ResourceHook targetHook)
        {
            var dupes = meta.Where(pair => pair.Key.Contains("DUPLICATE")).Select(pair => pair.Key);
            foreach (string target in dupes)
            {
                meta.Remove(target);
            }

            var noHookImplementation = meta.Where(pair => pair.Value.Item2 == null) // do something with ShouldExecute(targethook) for related type.
                        .Select(pair => pair.Key);
            foreach (string target in noHookImplementation)
            {
                meta.Remove(target);
            }
        }

        IResourceHookContainer<TEntity> GetResourceDefinition(Type targetEntity)
        {
            return (IResourceHookContainer<TEntity>)_genericProcessorFactory.GetProcessor<IResourceDefinition>(typeof(ResourceDefinition<>), targetEntity);
        }

        //virtual public IList<TEntity> ExecuteHook(IList<TEntity> entities, string rel)
        //{
        //    // seeing as the relationships are already processed, we can just do
        //    // Logic.{method}(articles.Tags)

        //    var entity = _jsonApiContext.RequestEntity;

        //    var relationship = entity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == rel);
        //    Type l1 = typeof(List<>);
        //    if (relationship.GetType() == typeof(HasManyThroughAttribute))
        //    {
        //        var castRelationship = relationship as HasManyThroughAttribute;
        //        object nestedLogic;
        //        for (int index = 0; index < entities.Count(); ++index)
        //        {
        //            // this is our {Article}
        //            var listEntity = entities[index];
        //            // Get the {Article.ArticleTags}
        //            var relevantProperty = listEntity.GetType().GetProperty(castRelationship.InternalThroughName, BindingFlags.Public | BindingFlags.Instance);
        //            var intermediateEntities = relevantProperty.GetValue(listEntity) as IList;

        //            // Logic for this nested property
        //            nestedLogic = GetLogic(castRelationship.Type);

        //            // We default to OnList at the moment
        //            var method = nestedLogic.GetType().GetMethods().First(e => e.Name == "OnList");

        //            // get Tags, this can be replaced with some SelectMany's
        //            Type constructed = l1.MakeGenericType(castRelationship.RightProperty.PropertyType);

        //            // Not happy with this, but was needed.
        //            IList toHold = Activator.CreateInstance(constructed) as IList;
        //            foreach (var iEntity in intermediateEntities)
        //            {
        //                // Iterating over the {ArticleTag}s to get all the {Tag}s
        //                var fetchedTags = (iEntity.GetType().GetProperty(castRelationship.RightProperty.Name, BindingFlags.Public | BindingFlags.Instance).GetValue(iEntity));
        //                toHold.Add(fetchedTags);
        //            }
        //            //

        //            IEnumerable filteredTags = method.Invoke(nestedLogic, new object[] { toHold }) as IEnumerable;
        //            toHold = filteredTags as IList;

        //            var toHoldHashSet = (HashSet<dynamic>)GetHashSet((IEnumerable<dynamic>)filteredTags);
        //            // We no process all {Article.ArticleTags} in a for loop because we're changing it
        //            for (int i = 0; i < intermediateEntities.Count; i++)
        //            {
        //                var iEntity = intermediateEntities[i];
        //                var property = iEntity.GetType().GetProperty(castRelationship.RightProperty.Name, BindingFlags.Public | BindingFlags.Instance);

        //                var item = property.GetValue(iEntity);

        //                if (!toHoldHashSet.ToList().Contains(item))
        //                {
        //                    // What we first did:
        //                    //property.SetValue(iEntity, null);
        //                    // if {tag} is not filled, we shouldnt fill in {ArticleTag} because otherwise
        //                    // JsonApiDotNetCore will try to show a null value, resulting in object refrence not set to an instance of an object
        //                    // so we delete it here.
        //                    // We shouldnt remove, I'm just emulating the WHERE here...
        //                    intermediateEntities.RemoveAt(i);
        //                }
        //                else
        //                {
        //                    // dont need to do anything
        //                    // Maybe, in the future, when patches are done we need to do
        //                    // this:
        //                    // intermediateEntities[i] = iEntity;
        //                }
        //            }
        //            PropertyInfo prop = listEntity.GetType().GetProperty(castRelationship.InternalThroughName, BindingFlags.Public | BindingFlags.Instance);
        //            if (null != prop && prop.CanWrite)
        //            {
        //                prop.SetValue(listEntity, intermediateEntities);
        //            }


        //            entities[index] = listEntity;

        //        }


        //    }
        //    return entities;

        //}
        //private HashSet<T> GetHashSet<T>(IEnumerable<T> source)
        //{
        //    return new HashSet<T>(source);
        //}
        //private static List<T> ConvertList<T>(List<object> value, Type type)
        //{
        //    return new List<T>(value.Select(item => (T)Convert.ChangeType(item, type)));
        //}


        //private IQueryable<TType> CallLogic<TType>(IQueryable<TType> entities, IResourceDefinition resourceDefinition) where TType : class, IIdentifiable
        //{
        //    Type resourceType = resourceDefinition.GetType();
        //    MethodInfo getMethod = resourceType.GetMethod("OnList", BindingFlags.Public);
        //    MethodInfo genericGet = getMethod.MakeGenericMethod(new[] { typeof(TType) });


        //    return (IQueryable<TType>)genericGet.Invoke(resourceType, new object[] { entities });
        //}






    }
}
