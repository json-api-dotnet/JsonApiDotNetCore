using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Create hooks container
    /// </summary>
    public interface ICreateHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before creation of resources of type
        /// <typeparamref name="TResource" />.
        /// <para />
        /// For the <see cref="ResourcePipeline.Post" /> pipeline, <paramref name="resources" /> will typically contain one entry.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}" /> may be a subset of <paramref name="resources" />, in which case the operation of the pipeline will
        /// not be executed for the omitted resources. The returned set may also contain custom changes of the properties on the resources.
        /// <para />
        /// If new relationships are to be created with the to-be-created resources, this will be reflected by the corresponding NavigationProperty being set.
        /// For each of these relationships, the <see cref="ResourceHooksDefinition{TResource}.BeforeUpdateRelationship" /> hook is fired after the execution of
        /// this hook.
        /// </summary>
        /// <returns>
        /// The transformed resource set
        /// </returns>
        /// <param name="resources">
        /// The unique set of affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        IEnumerable<TResource> BeforeCreate(IResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just after creation of resources of type
        /// <typeparamref name="TResource" />.
        /// <para />
        /// If relationships were created with the created resources, this will be reflected by the corresponding NavigationProperty being set. For each of these
        /// relationships, the <see cref="ResourceHooksDefinition{TResource}.AfterUpdateRelationship(IRelationshipsDictionary{TResource}, ResourcePipeline)" />
        /// hook is fired after the execution of this hook.
        /// </summary>
        /// <returns>
        /// The transformed resource set
        /// </returns>
        /// <param name="resources">
        /// The unique set of affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        void AfterCreate(HashSet<TResource> resources, ResourcePipeline pipeline);
    }
}
