using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Wrapper interface for all On execution methods.
    /// </summary>
    public interface IOnReturnHookExecutor
    {
        /// <summary>
        /// Executes the On Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.OnReturn" /> for every unique resource type occurring in parameter
        /// <paramref name="resources" />.
        /// </summary>
        /// <returns>
        /// The transformed set
        /// </returns>
        /// <param name="resources">
        /// Target resources for the Before cycle.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <typeparam name="TResource">
        /// The type of the root resources
        /// </typeparam>
        IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;
    }
}
