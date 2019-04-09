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
    /// <summary>
    /// A utility class responsible for executing hooks as defined in 
    /// the ResourceDefinition<typeparamref name="TEntity"/>> class (eg. OnList)
    /// when the  POST, GET, PATC, DELETE etc pipelines are executed.
    /// </summary>
    public class ResourceHookExecutor<TEntity> : IResourceHookExecutor<TEntity> where TEntity : class, IIdentifiable
    {

        protected readonly ResourceHook[] _implementedHooks;
        protected readonly IJsonApiContext _jsonApiContext;
        protected readonly IGenericProcessorFactory _genericProcessorFactory;
        protected readonly ResourceDefinition<TEntity> _resourceDefinition;

        public ResourceHookExecutor(IJsonApiContext jsonApiContext, IImplementedResourceHooks<TEntity> hooksConfiguration)
        {
            _genericProcessorFactory = jsonApiContext.GenericProcessorFactory;
            _jsonApiContext = jsonApiContext;
            _implementedHooks = hooksConfiguration.ImplementedHooks;
        }

        /// <inheritdoc/>
        public virtual bool ShouldExecuteHook(ResourceHook hook)
        {
            return _implementedHooks.Contains(hook);
        }

        /// <inheritdoc/>
        public virtual TEntity BeforeCreate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.BeforeCreate)) return entity;

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual TEntity AfterCreate(TEntity entity, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterCreate)) return entity;

            throw new NotImplementedException();
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
            var hookContainer = GetResourceDefinition(typeof(TEntity));
            hookContainer.BeforeDelete(entity, actionSource);
        }

        /// <inheritdoc/>
        public virtual void AfterDelete(TEntity entity, bool succeeded, ResourceAction actionSource)
        {
            if (!ShouldExecuteHook(ResourceHook.AfterDelete)) return;
            var hookContainer = GetResourceDefinition(typeof(TEntity));
            hookContainer.AfterDelete(entity, succeeded, actionSource);
        }


        private IResourceHookContainer<TEntity> GetResourceDefinition(Type targetEntity)
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
