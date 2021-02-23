using System.Collections.Generic;
using System.Threading;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Wrapper interface for all Before execution methods.
    /// </summary>
    public interface IReadHookExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeRead" /> hook for the requested resources as well as any related relationship.
        /// </summary>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="stringId">
        /// StringId of the requested resource in the case of <see cref="JsonApiResourceService{TResource,TId}.GetAsync(TId, CancellationToken)" />.
        /// </param>
        /// <typeparam name="TResource">
        /// The type of the request resource
        /// </typeparam>
        void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Executes the After Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.AfterRead" /> for every unique resource type occurring in parameter
        /// <paramref name="resources" />.
        /// </summary>
        /// <param name="resources">
        /// Target resources for the Before cycle.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <typeparam name="TResource">
        /// The type of the root resources
        /// </typeparam>
        void AfterRead<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;
    }
}
