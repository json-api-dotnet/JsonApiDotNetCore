using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Read hooks container
    /// </summary>
    public interface IReadHookContainer<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just before reading resources of type
        /// <typeparamref name="TResource" />.
        /// </summary>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="isIncluded">
        /// Indicates whether the to be queried resources are the primary request resources or if they were included
        /// </param>
        /// <param name="stringId">
        /// The string ID of the requested resource, in the case of <see cref="ResourcePipeline.GetSingle" />
        /// </param>
        void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null);

        /// <summary>
        /// Implement this hook to run custom logic in the <see cref=" JsonApiResourceService{TResource}" /> layer just after reading resources of type
        /// <typeparamref name="TResource" />.
        /// </summary>
        /// <param name="resources">
        /// The unique set of affected resources.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="isIncluded">
        /// A boolean to indicate whether the resources in this hook execution are the primary resources of the request, or if they were included as a
        /// relationship
        /// </param>
        void AfterRead(HashSet<TResource> resources, ResourcePipeline pipeline, bool isIncluded = false);
    }
}
