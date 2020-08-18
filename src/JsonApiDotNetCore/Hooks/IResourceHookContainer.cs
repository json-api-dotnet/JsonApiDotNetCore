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
    /// Implement this interface to implement business logic hooks on <see cref="ResourceDefinition{TResource}"/>.
    /// </summary>
    public interface IResourceHookContainer<TResource>
        : IReadHookContainer<TResource>, IDeleteHookContainer<TResource>, ICreateHookContainer<TResource>,
          IUpdateHookContainer<TResource>, IOnReturnHookContainer<TResource>, IResourceHookContainer
        where TResource : class, IIdentifiable { }

    /// <summary>
    /// Read hooks container
    /// </summary>
    public interface IReadHookContainer<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before reading resources of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="isIncluded">Indicates whether the to be queried resources are the primary request resources or if they were included</param>
        /// <param name="stringId">The string id of the requested resource, in the case of <see cref="ResourcePipeline.GetSingle"/></param>
        void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null);
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just after reading resources of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="isIncluded">A boolean to indicate whether the resources in this hook execution are the primary resources of the request, 
        /// or if they were included as a relationship</param>
        void AfterRead(HashSet<TResource> resources, ResourcePipeline pipeline, bool isIncluded = false);
    }

    /// <summary>
    /// Create hooks container
    /// </summary>
    public interface ICreateHookContainer<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before creation of resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Post"/> pipeline, <paramref name="resources"/> 
        /// will typically contain one entry.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}"/> may be a subset 
        /// of <paramref name="resources"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted resources. The returned 
        /// set may also contain custom changes of the properties on the resources.
        /// <para />
        /// If new relationships are to be created with the to-be-created resources,
        /// this will be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <returns>The transformed resource set</returns>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeCreate(IResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just after creation of resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// If relationships were created with the created resources, this will
        /// be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.AfterUpdateRelationship(IRelationshipsDictionary{T}, ResourcePipeline)"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <returns>The transformed resource set</returns>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterCreate(HashSet<TResource> resources, ResourcePipeline pipeline);
    }

    /// <summary>
    /// update hooks container
    /// </summary>
    public interface IUpdateHookContainer<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before updating resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Patch"/> pipeline, the
        /// <paramref name="resources" /> will typically contain one resource. 
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}"/> may be a subset 
        /// of the <see cref="DiffableResourceHashSet{TResource}"/> property in parameter <paramref name="resources"/>, 
        /// in which case the operation of the  pipeline will not be executed 
        /// for the omitted resources. The returned set may also contain custom 
        /// changes of the properties on the resources.
        /// <para />
        /// If new relationships are to be created with the to-be-updated resources,
        /// this will be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.BeforeUpdateRelationship"/>
        /// hook is fired after the execution of this hook.
        /// <para />
        /// If by the creation of these relationships, any other relationships (eg
        /// in the case of an already populated one-to-one relationship) are implicitly 
        /// affected, the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these.
        /// </summary>
        /// <returns>The transformed resource set</returns>
        /// <param name="resources">The affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeUpdate(IDiffableResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before updating relationships to resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// This hook is fired when a relationship is created to resources of type 
        /// <typeparamref name="TResource"/> from a dependent pipeline (<see cref="ResourcePipeline.Post"/>
        /// or <see cref="ResourcePipeline.Patch"/>). For example, If an Article was created
        /// and its author relationship was set to an existing Person, this hook will be fired
        /// for that particular Person.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}"/> may be a subset 
        /// of <paramref name="ids"/>, in which case the operation of the 
        /// pipeline will not be executed for any resource whose id was omitted
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="ids">The unique set of ids</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="resourcesByRelationship">A helper that groups the resources by the affected relationship</param>
        IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just after updating resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// If relationships were updated with the updated resources, this will
        /// be reflected by the corresponding NavigationProperty being set. 
        /// For each of these relationships, the <see cref="ResourceDefinition{T}.AfterUpdateRelationship(IRelationshipsDictionary{T}, ResourcePipeline)"/>
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterUpdate(HashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> layer
        /// just after a relationship was updated.
        /// </summary>
        /// <param name="resourcesByRelationship">Relationship helper.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void AfterUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before implicitly updating relationships to resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// This hook is fired when a relationship to resources of type 
        /// <typeparamref name="TResource"/> is implicitly affected from a dependent pipeline (<see cref="ResourcePipeline.Patch"/>
        /// or <see cref="ResourcePipeline.Delete"/>). For example, if an Article was updated
        /// by setting its author relationship (one-to-one) to an existing Person, 
        /// and by this the relationship to a different Person was implicitly removed, 
        /// this hook will be fired for the latter Person.
        /// <para />
        /// See <see cref="ResourceDefinition{TResource}.BeforeUpdate(IDiffableResourceHashSet{TResource},ResourcePipeline)"/> for information about
        /// when this hook is fired.
        /// <para />
        /// </summary>
        /// <returns>The transformed set of ids</returns>
        /// <param name="resourcesByRelationship">A helper that groups the resources by the affected relationship</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline);
    }

    /// <summary>
    /// Delete hooks container
    /// </summary>
    public interface IDeleteHookContainer<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just before deleting resources of type <typeparamref name="TResource"/>.
        /// <para />
        /// For the <see cref="ResourcePipeline.Delete"/> pipeline,
        /// <paramref name="resources" /> will typically contain one resource. 
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}"/> may be a subset 
        /// of <paramref name="resources"/>, in which case the operation of the 
        /// pipeline will not be executed for the omitted resources.
        /// <para />
        /// If by the deletion of these resources any other resources are affected 
        /// implicitly by the removal of their relationships (eg
        /// in the case of an one-to-one relationship), the <see cref="ResourceDefinition{T}.BeforeImplicitUpdateRelationship"/>
        /// hook is fired for these resources.
        /// </summary>
        /// <returns>The transformed resource set</returns>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}"/> 
        /// layer just after deletion of resources of type <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="resources">The unique set of affected resources.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="succeeded">If set to <c>true</c> the deletion succeeded in the repository layer.</param>
        void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded);
    }

    /// <summary>
    /// On return hook container
    /// </summary>
    public interface IOnReturnHookContainer<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to transform the result data just before returning
        /// the resources of type <typeparamref name="TResource"/> from the 
        /// <see cref=" JsonApiResourceService{TResource}"/> layer
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}"/> may be a subset 
        /// of <paramref name="resources"/> and may contain changes in properties
        /// of the encapsulated resources. 
        /// <para />
        /// </summary>
        /// <returns>The transformed resource set</returns>
        /// <param name="resources">The unique set of affected resources</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        IEnumerable<TResource> OnReturn(HashSet<TResource> resources, ResourcePipeline pipeline);
    }
}
