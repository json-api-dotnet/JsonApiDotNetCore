using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{


    public interface IResourceHookContainer<T> where T : class, IIdentifiable
    {
        void BeforeGet();
        IEnumerable<T> AfterGet(List<T> entities);
        void BeforeGetSingle(string stringId);
        T AfterGetSingle(T entity);
        T BeforeCreate(T entity);
        void AfterCreate(T entity);
        T BeforeUpdate(T entity);
        void AfterUpdate(T entity);
        void BeforeDelete(T entity);
        void AfterDelete(T entity);
        void BeforeGetRelationship(string stringId, string relationshipName);
        T AfterGetRelationship(T entity);
        void BeforeUpdateRelationships(T entity, string relationshipName, List<object> relationships);
        void AfterUpdateRelationships(T entity, string relationshipName, List<object> relationships);

        // See the comments of the method implementation for details on this.
        // IQueryable<T> OnQueryGet(IQueryable<T> entities);
    }

    public interface IResourceHookExecutor<T> : IResourceHookContainer<T> where T : class, IIdentifiable
    {
        bool ShouldExecuteHook(ResourceHook hook);

    }
}