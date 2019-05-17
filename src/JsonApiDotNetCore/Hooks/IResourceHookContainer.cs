using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;


namespace JsonApiDotNetCore.Hooks
{
    public interface IResourceHookContainer { }

    public interface IResourceHookContainer<TEntity> : IBeforeHooks<TEntity>, IAfterHooks<TEntity>, IOnHooks<TEntity>, IResourceHookContainer where TEntity : class, IIdentifiable { }

    public interface IAfterHooks<TEntity> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after creation of entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// If along with the created entities there were also updated relationships, 
        /// this is reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is also fired.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterCreate(HashSet<TEntity> entities, ResourceAction pipeline);
        void AfterRead(HashSet<TEntity> entities, ResourceAction pipeline, bool isIncluded = false);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after creation of entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// If along with the created entities there were also updated relationships, 
        /// this is reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is also fired.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterUpdate(HashSet<TEntity> entities, ResourceAction pipeline);
        void AfterDelete(HashSet<TEntity> entities, ResourceAction pipeline, bool succeeded);
        void AfterUpdateRelationship(IUpdatedRelationshipHelper<TEntity> relationshipHelper, ResourceAction pipeline);
    }

    public interface IBeforeHooks<TEntity> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before creation of entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// For the <see cref="ResourceAction.Create"/> pipeline, <paramref name="entities"/> 
        /// will typically contain one entry. For <see cref="ResourceAction.BulkCreate"/>, 
        /// <paramref name="entities"/> can contain multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted entities. The returned 
        /// set may also contain custom changes of the properties on the entities.
        /// <para />
        /// If along with the to be created entities there are also relationships
        /// to be created, this is reflected by the corresponding NavigationProperty
        /// being set. For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is also fired.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TEntity> BeforeCreate(HashSet<TEntity> entities, ResourceAction pipeline);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before reading entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="isIncluded">Indicates whether the to be queried entities are the main request entities or if they were included</param>
        /// <param name="stringId">The string id of the requested entity, in the case of <see cref="ResourceAction.GetSingle"/></param>
        void BeforeRead(ResourceAction pipeline, bool isIncluded = false, string stringId = null);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before updating entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// For the <see cref="ResourceAction.Patch"/> pipeline, the
        /// <paramref name="entityDiff" /> will typically contain one entity. 
        /// For <see cref="ResourceAction.BulkPatch"/>, this it may contain 
        /// multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted entities. The returned 
        /// set may also contain custom changes of the properties on the entities.
        /// <para />
        /// If along with the to be created entities there are also relationships
        /// to be created, this is reflected by the corresponding NavigationProperty
        /// being set. For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is also fired.
        /// <para />
        /// If by the creation of these relationships, any other relationships (eg
        /// in the case of an already populated one-to-one relationship) are implicitly 
        /// affected, the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entityDiff">The entity diff.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TEntity> BeforeUpdate(EntityDiff<TEntity> entityDiff, ResourceAction pipeline);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before deleting entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// For the <see cref="ResourceAction.Delete"/> pipeline,
        /// <paramref name="entities" /> will typically contain one entity. 
        /// For <see cref="ResourceAction.BulkDelete"/>, this it may contain 
        /// multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted entities.
        /// <para />
        /// If along with the to be created entities there are also relationships
        /// to be created, this is reflected by the corresponding NavigationProperty
        /// being set. For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is also fired.
        /// <para />
        /// If by the deletion of these entities any other entities are affected 
        /// implicitly by the removal of their relationships (eg
        /// in the case of an one-to-one relationship), the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these entities.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TEntity> BeforeDelete(HashSet<TEntity> entities, ResourceAction pipeline);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before updating relationships to entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// This hook is fired when a set of entities in the <see cref="ResourceAction.Create"/>
        /// or <see cref="ResourceAction.Patch"/> pipeline, a relationship is created
        /// to entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="ids"/>, in which case the operation of the 
        /// pipeline will not be executed for the entities with the omitted ids.
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="ids">The unique set of ids</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="relationshipHelper">A helper that groups the entities by the affected relationship</param>
        IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IUpdatedRelationshipHelper<TEntity> relationshipHelper, ResourceAction pipeline);
        /// <summary>
        /// Implement this hook to run logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before implicitly updating relationships to entities of type <typeparamref name="TEntity"/>.
        /// <para />
        /// See <see cref="ResourceDefinition{T}.BeforeUpdate"/> for information about
        /// when this hook is fired.
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="relationshipHelper">A helper that groups the entities by the affected relationship</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void BeforeImplicitUpdateRelationship(IUpdatedRelationshipHelper<TEntity> relationshipHelper, ResourceAction pipeline);
    }

    public interface IOnHooks<TEntity> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to transform the result data just before returning
        /// the entities of type <typeparamref name="TEntity"/> from the 
        /// <see cref=" EntityResourceService{T}"/> layer
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/> and may contain changes in properties
        /// of the encapsulated entities. 
        /// <para />
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TEntity> OnReturn(HashSet<TEntity> entities, ResourceAction pipeline);
    }
}
