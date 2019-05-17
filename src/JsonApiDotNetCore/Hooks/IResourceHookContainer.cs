using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    public interface IResourceHookContainer { }

    public interface IResourceHookContainer<T> : IBeforeHooks<T>, IAfterHooks<T>, IOnHooks<T>, IResourceHookContainer where T : class, IIdentifiable { }

    public interface IAfterHooks<T> where T : class, IIdentifiable
    {
        void AfterCreate(HashSet<T> entities, ResourceAction pipeline);
        void AfterRead(HashSet<T> entities, ResourceAction pipeline, bool isRelated = false);
        void AfterUpdate(HashSet<T> entities, ResourceAction pipeline);
        void AfterDelete(HashSet<T> entities, ResourceAction pipeline, bool succeeded);
        void AfterUpdateRelationship(IUpdatedRelationshipHelper<T> relationshipHelper, ResourceAction pipeline);
    }

    public interface IBeforeHooks<T> where T : class, IIdentifiable
    {
        IEnumerable<T> BeforeCreate(HashSet<T> entities, ResourceAction pipeline);
        void BeforeRead(ResourceAction pipeline, bool nestedHook = false, string stringId = null);
        IEnumerable<T> BeforeUpdate(EntityDiff<T> entityDiff, ResourceAction pipeline);
        IEnumerable<T> BeforeDelete(HashSet<T> entities, ResourceAction pipeline);
        IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IUpdatedRelationshipHelper<T> relationshipHelper, ResourceAction pipeline);
        void BeforeImplicitUpdateRelationship(IUpdatedRelationshipHelper<T> relationshipHelper, ResourceAction pipeline);
    }

    public interface IOnHooks<T> where T : class, IIdentifiable
    {
        IEnumerable<T> OnReturn(HashSet<T> entities, ResourceAction pipeline);
    }
}
