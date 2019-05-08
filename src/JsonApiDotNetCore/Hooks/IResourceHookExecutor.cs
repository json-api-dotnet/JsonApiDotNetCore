using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{

    public interface IResourceHookContainer
    {

    }

    public interface IResourceHookContainer<T> : IResourceHookContainer where T : class, IIdentifiable
    {
        /// <summary>
        /// A hook executed before creating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be created relationships, the BeforeUpdate 
        /// and AfterUpdate hooks for those relations will be called too.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.CreateAsync
        /// </summary>
        /// <returns>The (adjusted) entities to be created</returns>
        /// <param name="entities">The entities to be created</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> BeforeCreate(EntityDiff<T> entityDiff, HookExecutionContext<T> context);

        /// <summary>
        /// A hook executed after creating an entity. Can be used eg. for publishing events.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.CreateAsync
        /// </summary>
        /// <param name="entities">The entities that were created</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> AfterCreate(IEnumerable<T> entities, HookExecutionContext<T> context);

        /// <summary>
        /// A hook executed after before reading entities. Can be used eg. for logging, authorization.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.GetAsync()
        ///     * EntityResourceService.GetAsync(TId id)
        ///     * GetRelationshipsAsync.GetAsync(TId id)
        /// 
        /// </summary>
        /// <param name="actionSource">The entities that result from the query</param>
        /// <param name="stringId">If the </param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        void BeforeRead(ResourceAction pipeline, bool nestedHook = false, string stringId = null);

        /// <summary>
        /// A hook executed after reading entities. Can be used eg. for publishing events.
        /// 
        /// Can also be used to to filter on the result set of a custom include. For example,
        /// if Articles -> Blogs -> Tags are retrieved, the AfterRead method as defined in
        /// in all related ResourceDefinitions (if present) will be called: 
        ///     * first for all articles;
        ///     * then for all blogs;
        ///     * lastly for all tags. 
        /// This can be used to build an in-memory filtered include, which is not yet suported by EF Core, 
        /// <see href="https://github.com/aspnet/EntityFrameworkCore/issues/1833">see this issue</see>.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.GetAsync()
        ///     * EntityResourceService.GetAsync(TId id)
        ///     * GetRelationshipsAsync.GetAsync(TId id)
        /// </summary>
        /// <returns>The (adjusted) entities that result from the query</returns>
        /// <param name="entities">The entities that result from the query</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> AfterRead(IEnumerable<T> entities, ResourceAction pipeline, bool nestedHook = false);

        /// <summary>
        /// A hook executed before updating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be updated relationships, the BeforeUpdate()
        /// and AfterUdpate() hooks for the entities of those relationships are also executed.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.UpdateAsync
        /// </summary>
        /// <returns>The (adjusted) entities to be updated</returns>
        /// <param name="entities">The entities to be updated</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> BeforeUpdate(EntityDiff<T> entityDiff, HookExecutionContext<T> context);

        /// <summary>
        /// A hook executed after updating an entity. Can be used eg. for publishing an event.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.UpdateAsync
        /// </summary>
        /// <param name="entities">The entities that were updated</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> AfterUpdate(IEnumerable<T> entities, HookExecutionContext<T> context);

        /// <summary>
        /// A hook executed before deleting an entity. Can be used eg. for authorization.
        /// </summary>
        /// <param name="entities">The entities to be deleted</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<T> BeforeDelete(IEnumerable<T> entities, HookExecutionContext<T> context);

        /// <summary>
        /// A hook executed before deleting an entity. Can be used eg. for publishing an event.
        /// </summary>
        /// <param name="entities">The entities to be deleted</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        /// <param name="succeeded">A boolean to indicate whether the deletion was succesful</param>
        IEnumerable<T> AfterDelete(IEnumerable<T> entities, HookExecutionContext<T> context, bool succeeded);

        /// <summary>
        /// TODO: WRITE ME
        /// </summary>
        /// <param name="entities">The entities subjected to an implicit update</param>
        /// <param name="context">The pipeline from which the hook was called</param>
        void ImplicitUpdateRelationship(IEnumerable<T> entities, RelationshipAttribute affectedRelationship);
    }


    /// <summary>
    /// A utility class responsible for executing hooks as defined in 
    /// the IResourceHookContainer<typeparamref name="T"/>.
    /// 
    /// The hook execution flow is as follows:
    /// 1.  The EntityResourceService<typeparamref name="T"/> instance (service instance)
    /// holds a reference (through dependency injection) to the executor instance.
    /// 2.  When the eg. DeleteAsync() method on the service instance is called, the service instance
    /// calls the BeforeDelete and AfterDelete methods on the hook executor instance.
    /// 3.  The hook executor instance is then responsible for getting access to and calling the hooks
    /// that are defined on IResourceHookContainer<typeparamref name="T"/> (which
    /// by default is the ResourceDefinition implementation).
    /// 4. Note that for the simple case of service{Model}.DeleteAsync(), only 
    /// ResourceDefinition{Model} is involved. But for more complex operations, like that
    /// of service{Model}.GetAsync() with a nested include of related entities, ResourceDefintions for all
    /// involved models are resolved and used in a more complex travesal of the resultset.
    /// </summary>
    public interface IResourceHookExecutor 
    {
        /// <summary>
        /// A hook executed before creating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be created relationships, the BeforeUpdate 
        /// and AfterUpdate hooks for those relations will be called too.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.CreateAsync
        /// </summary>
        /// <returns>The (adjusted) entities to be created</returns>
        /// <param name="entities">The entities to be created</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed after creating an entity. Can be used eg. for publishing events.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.CreateAsync
        /// </summary>
        /// <param name="entities">The entities that were created</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed after before reading entities. Can be used eg. for logging, authorization.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.GetAsync()
        ///     * EntityResourceService.GetAsync(TId id)
        ///     * GetRelationshipsAsync.GetAsync(TId id)
        /// 
        /// </summary>
        /// <param name="actionSource">The entities that result from the query</param>
        /// <param name="stringId">If the </param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        void BeforeRead<TEntity>(ResourceAction actionSource, string stringId = null) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed after reading entities. Can be used eg. for publishing events.
        /// 
        /// Can also be used to to filter on the result set of a custom include. For example,
        /// if Articles -> Blogs -> Tags are retrieved, the AfterRead method as defined in
        /// in all related ResourceDefinitions (if present) will be called: 
        ///     * first for all articles;
        ///     * then for all blogs;
        ///     * lastly for all tags. 
        /// This can be used to build an in-memory filtered include, which is not yet suported by EF Core, 
        /// <see href="https://github.com/aspnet/EntityFrameworkCore/issues/1833">see this issue</see>.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.GetAsync()
        ///     * EntityResourceService.GetAsync(TId id)
        ///     * GetRelationshipsAsync.GetAsync(TId id)
        /// </summary>
        /// <returns>The (adjusted) entities that result from the query</returns>
        /// <param name="entities">The entities that result from the query</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed before updating an entity. Can be used eg. for authorization.
        /// If the entity also contains to be updated relationships, the BeforeUpdate()
        /// and AfterUdpate() hooks for the entities of those relationships are also executed.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.UpdateAsync
        /// </summary>
        /// <returns>The (adjusted) entities to be updated</returns>
        /// <param name="entities">The entities to be updated</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed after updating an entity. Can be used eg. for publishing an event.
        /// 
        /// This hook is executed from:
        ///     * EntityResourceService.UpdateAsync
        /// </summary>
        /// <param name="entities">The entities that were updated</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed before deleting an entity. Can be used eg. for authorization.
        /// </summary>
        /// <param name="entities">The entities to be deleted</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        IEnumerable<TEntity> BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource) where TEntity : class, IIdentifiable;

        /// <summary>
        /// A hook executed before deleting an entity. Can be used eg. for publishing an event.
        /// </summary>
        /// <param name="entities">The entities to be deleted</param>
        /// <param name="actionSource">The pipeline from which the hook was called</param>
        /// <param name="succeeded">A boolean to indicate whether the deletion was succesful</param>
        IEnumerable<TEntity> AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction actionSource, bool succeeded) where TEntity : class, IIdentifiable;
    }



    public interface IResourceHookContainerNew<T> : IResourceHookContainer where T : class, IIdentifiable
    {
        void BeforeRead(HookExecutionContext<T> context, string stringId = null);
        IEnumerable<T> AfterRead(IEnumerable<T> entitiesInDb, HookExecutionContext<T> context);
        IEnumerable<T> BeforeCreate(IEnumerable<T> entitiesFromRequest, HookExecutionContext<T> context);
        IEnumerable<T> BeforeUpdate(EntityDiff<T> entityDiff, HookExecutionContext<T> context);
        IEnumerable<T> BeforeUpdateRelationship(IEnumerable<T> entities, HookExecutionContext<T> context);
        IEnumerable<T> AfterCreate(IEnumerable<T> entitiesInDb, HookExecutionContext<T> context);
        IEnumerable<T> AfterUpdate(IEnumerable<T> entitiesInDb, HookExecutionContext<T> context);
        IEnumerable<T> BeforeDelete(IEnumerable<T> entitiesInDb, HookExecutionContext<T> context);
        IEnumerable<T> AfterDelete(IEnumerable<T> entitiesInDb, HookExecutionContext<T> context, bool succeeded);
    }
}