using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Not meant for public usage. Used internally in the <see cref="ResourceHookExecutor"/>
    /// </summary>
    public interface IResourceHookContainer { }

    /// <summary>
    /// Implement this interface to implement business logic hooks on <see cref="ResourceDefinition{T}"/>.
    /// </summary>
    public interface IResourceHookContainer<TResource> : IBeforeHooks<TResource>, IAfterHooks<TResource>, IOnHooks<TResource>, IResourceHookContainer where TResource : class, IIdentifiable { }

    /// <summary>
    /// Wrapper interface for all Before hooks.
    /// </summary>
    public interface IBeforeHooks<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before creation of entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Post"/> pipeline, <paramref name="entities"/> 
        /// will typically contain one entry. For <see cref="ResourcePipeline.BulkCreate"/>, 
        /// <paramref name="entities"/> can contain multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted entities. The returned 
        /// set may also contain custom changes of the properties on the entities.
        /// <para />
        /// If new relationships are to be created with the to-be-created entities,
        /// this will be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeCreate(IEntityHashSet<TResource> entities, ResourcePipeline pipeline);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before reading entities of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="isIncluded">Indicates whether the to be queried entities are the main request entities or if they were included</param>
        /// <param name="stringId">The string id of the requested entity, in the case of <see cref="ResourcePipeline.GetSingle"/></param>
        void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before updating entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Patch"/> pipeline, the
        /// <paramref name="entityDiff" /> will typically contain one entity. 
        /// For <see cref="ResourcePipeline.BulkPatch"/>, this it may contain 
        /// multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of the <see cref="EntityDiffs{TEntity}.RequestEntities"/> property in parameter <paramref name="entityDiff"/>, 
        /// in which case the operation of the  pipeline will not be executed 
        /// for the omitted entities. The returned set may also contain custom 
        /// changes of the properties on the entities.
        /// <para />
        /// If new relationships are to be created with the to-be-updated entities,
        /// this will be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is fired after the execution of this hook.
        /// <para />
        /// If by the creation of these relationships, any other relationships (eg
        /// in the case of an already populated one-to-one relationship) are implicitly 
        /// affected, the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entityDiff">The entity diff.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeUpdate(IEntityDiff<TResource> entityDiff, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before deleting entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Delete"/> pipeline,
        /// <paramref name="entities" /> will typically contain one entity. 
        /// For <see cref="ResourcePipeline.BulkDelete"/>, this it may contain 
        /// multiple entities.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="entities"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted entities.
        /// <para />
        /// If by the deletion of these entities any other entities are affected 
        /// implicitly by the removal of their relationships (eg
        /// in the case of an one-to-one relationship), the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these entities.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeDelete(IEntityHashSet<TResource> entities, ResourcePipeline pipeline);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before updating relationships to entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// This hook is fired when a relationship is created to entities of type 
        /// <typeparamref name="TResource"/> from a dependent pipeline (<see cref="ResourcePipeline.Post"/>
        /// or <see cref="ResourcePipeline.Patch"/>). For example, If an Article was created
        /// and its author relationship was set to an existing Person, this hook will be fired
        /// for that particular Person.
        /// <para />
        /// The returned <see cref="IEnumerable{TEntity}"/> may be a subset 
        /// of <paramref name="ids"/>, in which case the operation of the 
        /// pipeline will not be executed for any entity whose id was omitted
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="ids">The unique set of ids</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="entitiesByRelationship">A helper that groups the entities by the affected relationship</param>
        IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just before implicitly updating relationships to entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// This hook is fired when a relationship to entities of type 
        /// <typeparamref name="TResource"/> is implicitly affected from a dependent pipeline (<see cref="ResourcePipeline.Patch"/>
        /// or <see cref="ResourcePipeline.Delete"/>). For example, if an Article was updated
        /// by setting its author relationship (one-to-one) to an existing Person, 
        /// and by this the relationship to a different Person was implicitly removed, 
        /// this hook will be fired for the latter Person.
        /// <para />
        /// See <see cref="ResourceDefinition{T}.BeforeUpdate"/> for information about
        /// when this hook is fired.
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="entitiesByRelationship">A helper that groups the entities by the affected relationship</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline);
    }

    /// <summary>
    /// Wrapper interface for all After hooks.
    /// </summary>
    public interface IAfterHooks<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after creation of entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// If relationships were created with the created entities, this will
        /// be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.AfterUpdateRelationship(IRelationshipsDictionary{T}, ResourcePipeline)"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <returns>The transformed entity set</returns>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterCreate(HashSet<TResource> entities, ResourcePipeline pipeline);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after reading entities of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="isIncluded">A boolean to indicate whether the entities in this hook execution are the main entities of the request, 
        /// or if they were included as a relationship</param>
        void AfterRead(HashSet<TResource> entities, ResourcePipeline pipeline, bool isIncluded = false);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after updating entities of type <typeparamref name="TResource"/>.
        /// <para />
        /// If relationships were updated with the updated entities, this will
        /// be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.AfterUpdateRelationship(IRelationshipsDictionary{T}, ResourcePipeline"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterUpdate(HashSet<TResource> entities, ResourcePipeline pipeline);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> 
        /// layer just after deletion of entities of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="entities">The unique set of affected entities.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="succeeded">If set to <c>true</c> if the deletion was succeeded in the repository layer.</param>
        void AfterDelete(HashSet<TResource> entities, ResourcePipeline pipeline, bool succeeded);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" EntityResourceService{T}"/> layer
        /// just after a relationship was updated.
        /// </summary>
        /// <param name="entitiesByRelationship">Relationship helper.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterUpdateRelationship(IRelationshipsDictionary<TResource> entitiesByRelationship, ResourcePipeline pipeline);
    }

    /// <summary>
    /// Wrapper interface for all on hooks.
    /// </summary>
    public interface IOnHooks<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to transform the result data just before returning
        /// the entities of type <typeparamref name="TResource"/> from the 
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
        IEnumerable<TResource> OnReturn(HashSet<TResource> entities, ResourcePipeline pipeline);
    }
}
