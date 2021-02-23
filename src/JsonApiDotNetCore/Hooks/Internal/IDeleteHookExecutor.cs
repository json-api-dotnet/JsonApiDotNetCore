using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    public interface IDeleteHookExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropriate hooks if they are implemented. The returned set will be used in the actual operation in
        /// <see cref="JsonApiResourceService{TResource}" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeDelete" /> hook for values in parameter <paramref name="resources" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeImplicitUpdateRelationship" /> hook for any resources that are indirectly (implicitly)
        /// affected by this operation. Eg: when deleting a resource that has relationships set to other resources, these other resources are implicitly affected
        /// by the delete operation.
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
        IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Executes the After Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.AfterDelete" /> hook for values in parameter <paramref name="resources" />.
        /// </summary>
        /// <param name="resources">
        /// Target resources for the Before cycle.
        /// </param>
        /// <param name="pipeline">
        /// An enum indicating from where the hook was triggered.
        /// </param>
        /// <param name="succeeded">
        /// If set to <c>true</c> the deletion succeeded.
        /// </param>
        /// <typeparam name="TResource">
        /// The type of the root resources
        /// </typeparam>
        void AfterDelete<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline, bool succeeded)
            where TResource : class, IIdentifiable;
    }
}
