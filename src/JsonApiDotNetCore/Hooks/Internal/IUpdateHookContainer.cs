using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// update hooks container
    /// </summary>
    public interface IUpdateHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before updating resources of type
        /// <typeparamref name="TResource" />.
        /// <para />
        /// For the <see cref="ResourcePipeline.Patch" /> pipeline, the <paramref name="resources" /> will typically contain one resource.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}" /> may be a subset of the <see cref="DiffableResourceHashSet{TResource}" /> property in parameter
        /// <paramref name="resources" />, in which case the operation of the  pipeline will not be executed for the omitted resources. The returned set may also
        /// contain custom changes of the properties on the resources.
        /// <para />
        /// If new relationships are to be created with the to-be-updated resources, this will be reflected by the corresponding NavigationProperty being set.
        /// For each of these relationships, the <see cref="ResourceHooksDefinition{TResource}.BeforeUpdateRelationship" /> hook is fired after the execution of
        /// this hook.
        /// <para />
        /// If by the creation of these relationships, any other relationships (eg in the case of an already populated one-to-one relationship) are implicitly
        /// affected, the <see cref="ResourceHooksDefinition{TResource}.BeforeImplicitUpdateRelationship" /> hook is fired for these.
        /// </summary>
        /// <returns>
        /// The transformed resource set
        /// </returns>
        /// <param name="resources">
        /// The affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        IEnumerable<TResource> BeforeUpdate(IDiffableResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before updating relationships to
        /// resources of type <typeparamref name="TResource" />.
        /// <para />
        /// This hook is fired when a relationship is created to resources of type <typeparamref name="TResource" /> from a dependent pipeline (
        /// <see cref="ResourcePipeline.Post" /> or <see cref="ResourcePipeline.Patch" />). For example, If an Article was created and its author relationship
        /// was set to an existing Person, this hook will be fired for that particular Person.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}" /> may be a subset of <paramref name="ids" />, in which case the operation of the pipeline will not
        /// be executed for any resource whose ID was omitted
        /// <para />
        /// </summary>
        /// <returns>
        /// The transformed set of ids
        /// </returns>
        /// <param name="ids">
        /// The unique set of ids
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="resourcesByRelationship">
        /// A helper that groups the resources by the affected relationship
        /// </param>
        IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<TResource> resourcesByRelationship,
            ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just after updating resources of type
        /// <typeparamref name="TResource" />.
        /// <para />
        /// If relationships were updated with the updated resources, this will be reflected by the corresponding NavigationProperty being set. For each of these
        /// relationships, the <see cref="ResourceHooksDefinition{TResource}.AfterUpdateRelationship(IRelationshipsDictionary{TResource}, ResourcePipeline)" />
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <param name="resources">
        /// The unique set of affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        void AfterUpdate(HashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just after a relationship was updated.
        /// </summary>
        /// <param name="resourcesByRelationship">
        /// Relationship helper.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        void AfterUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before implicitly updating relationships
        /// to resources of type <typeparamref name="TResource" />.
        /// <para />
        /// This hook is fired when a relationship to resources of type <typeparamref name="TResource" /> is implicitly affected from a dependent pipeline (
        /// <see cref="ResourcePipeline.Patch" /> or <see cref="ResourcePipeline.Delete" />). For example, if an Article was updated by setting its author
        /// relationship (one-to-one) to an existing Person, and by this the relationship to a different Person was implicitly removed, this hook will be fired
        /// for the latter Person.
        /// <para />
        /// See <see cref="ResourceHooksDefinition{TResource}.BeforeUpdate(IDiffableResourceHashSet{TResource},ResourcePipeline)" /> for information about when
        /// this hook is fired.
        /// <para />
        /// </summary>
        /// <returns>
        /// The transformed set of ids
        /// </returns>
        /// <param name="resourcesByRelationship">
        /// A helper that groups the resources by the affected relationship
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<TResource> resourcesByRelationship, ResourcePipeline pipeline);
    }
}
