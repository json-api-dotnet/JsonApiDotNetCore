using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    public interface ICreateHookExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropriate hooks if they are implemented. The returned set will be used in the actual operation in
        /// <see cref="JsonApiResourceService{TResource}" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeCreate" /> hook for values in parameter <paramref name="resources" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeUpdateRelationship" /> hook for any secondary (nested) resource for values within
        /// parameter <paramref name="resources" />
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
        IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Executes the After Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.AfterCreate" /> hook for values in parameter <paramref name="resources" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.AfterUpdateRelationship" /> hook for any secondary (nested) resource for values within
        /// parameter <paramref name="resources" />
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
        void AfterCreate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;
    }
}
