using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Wrapper interface for all After execution methods.
    /// </summary>
    public interface IUpdateHookExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropriate hooks if they are implemented. The returned set will be used in the actual operation in
        /// <see cref="JsonApiResourceService{TResource}" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeUpdate(IDiffableResourceHashSet{TResource}, ResourcePipeline)" /> hook for values in
        /// parameter <paramref name="resources" />.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeUpdateRelationship" /> hook for any secondary (nested) resource for values within
        /// parameter <paramref name="resources" />
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.BeforeImplicitUpdateRelationship" /> hook for any resources that are indirectly (implicitly)
        /// affected by this operation. Eg: when updating a one-to-one relationship of a resource which already had this relationship populated, then this update
        /// will indirectly affect the existing relationship value.
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
        IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Executes the After Cycle by firing the appropriate hooks if they are implemented.
        /// <para />
        /// Fires the <see cref="ResourceHooksDefinition{TResource}.AfterUpdate" /> hook for values in parameter <paramref name="resources" />.
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
        void AfterUpdate<TResource>(IEnumerable<TResource> resources, ResourcePipeline pipeline)
            where TResource : class, IIdentifiable;
    }
}
