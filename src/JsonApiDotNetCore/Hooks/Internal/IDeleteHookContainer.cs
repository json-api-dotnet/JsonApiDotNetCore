using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Delete hooks container
    /// </summary>
    public interface IDeleteHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before deleting resources of type
        /// <typeparamref name="TResource" />.
        /// <para />
        /// For the <see cref="ResourcePipeline.Delete" /> pipeline, <paramref name="resources" /> will typically contain one resource.
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}" /> may be a subset of <paramref name="resources" />, in which case the operation of the pipeline will
        /// not be executed for the omitted resources.
        /// <para />
        /// If by the deletion of these resources any other resources are affected implicitly by the removal of their relationships (eg in the case of an
        /// one-to-one relationship), the <see cref="ResourceHooksDefinition{TResource}.BeforeImplicitUpdateRelationship" /> hook is fired for these resources.
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
        IEnumerable<TResource> BeforeDelete(IResourceHashSet<TResource> resources, ResourcePipeline pipeline);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just after deletion of resources of type
        /// <typeparamref name="TResource" />.
        /// </summary>
        /// <param name="resources">
        /// The unique set of affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="succeeded">
        /// If set to <c>true</c> the deletion succeeded in the repository layer.
        /// </param>
        void AfterDelete(HashSet<TResource> resources, ResourcePipeline pipeline, bool succeeded);
    }
}
