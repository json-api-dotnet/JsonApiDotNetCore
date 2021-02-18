using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// On return hook container
    /// </summary>
    public interface IOnReturnHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to transform the result data just before returning the resources of type <typeparamref name="TResource" /> from the
        /// <see cref=" JsonApiResourceService{TResource}" /> layer
        /// <para />
        /// The returned <see cref="IEnumerable{TResource}" /> may be a subset of <paramref name="resources" /> and may contain changes in properties of the
        /// encapsulated resources.
        /// <para />
        /// </summary>
        /// <returns>
        /// The transformed resource set
        /// </returns>
        /// <param name="resources">
        /// The unique set of affected resources
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        IEnumerable<TResource> OnReturn(HashSet<TResource> resources, ResourcePipeline pipeline);
    }
}
