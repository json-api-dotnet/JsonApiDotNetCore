using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Transient service responsible for executing Resource Hooks as defined 
    /// in <see cref="ResourceDefinition{T}"/>. see methods in 
    /// <see cref="IBeforeExecutor"/>, <see cref="IAfterExecutor"/> and 
    /// <see cref="IOnExecutor"/> for more information.
    /// 
    /// Uses <see cref="TraversalHelper"/> for traversal of nested entity data structures.
    /// Uses <see cref="HookExecutorHelper"/> for retrieving meta data about hooks, 
    /// fetching database values and performing other recurring internal operations.
    /// </summary>
    public interface IResourceHookExecutor : IBeforeExecutor, IAfterExecutor, IOnExecutor { }

    /// <summary>
    /// Wrapper interface for all Before execution methods.
    /// </summary>
    public interface IBeforeExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="DefaultResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeCreate"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        IEnumerable<TResource> BeforeCreate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeRead"/>
        /// hook where T = <typeparamref name="TResource"/> for the requested 
        /// entities as well as any related relationship.
        /// </summary>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="stringId">StringId of the requested entity in the case of 
        /// <see cref="DefaultResourceService{X, Y, Z}.GetAsync(Z)"/>.</param>
        /// <typeparam name="TResource">The type of the request entity</typeparam>
        void BeforeRead<TResource>(ResourcePipeline pipeline, string stringId = null) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="DefaultResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeUpdate(IDiffableEntityHashSet{T}, ResourcePipeline)"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeImplicitUpdateRelationship"/>
        /// hook for any entities that are indirectly (implicitly) affected by this operation.
        /// Eg: when updating a one-to-one relationship of an entity which already 
        /// had this relationship populated, then this update will indirectly affect 
        /// the existing relationship value.
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        IEnumerable<TResource> BeforeUpdate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="DefaultResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeDelete"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeImplicitUpdateRelationship"/>
        /// hook for any entities that are indirectly (implicitly) affected by this operation.
        /// Eg: when deleting an entity that has relationships set to other entities, 
        /// these other entities are implicitly affected by the delete operation.
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        IEnumerable<TResource> BeforeDelete<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
    }

    /// <summary>
    /// Wrapper interface for all After execution methods.
    /// </summary>
    public interface IAfterExecutor
    {
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterCreate"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.AfterUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        void AfterCreate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterRead"/> for every unique
        /// entity type occuring in parameter <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        void AfterRead<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterUpdate"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.AfterUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        void AfterUpdate<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterDelete"/>
        /// hook where T = <typeparamref name="TResource"/> for values in parameter <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        void AfterDelete<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline, bool succeeded) where TResource : class, IIdentifiable;
    }

    /// <summary>
    /// Wrapper interface for all On execution methods.
    /// </summary>
    public interface IOnExecutor
    {
        /// <summary>
        /// Executes the On Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.OnReturn"/> for every unique
        /// entity type occuring in parameter <paramref name="entities"/>.
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TResource">The type of the root entities</typeparam>
        IEnumerable<TResource> OnReturn<TResource>(IEnumerable<TResource> entities, ResourcePipeline pipeline) where TResource : class, IIdentifiable;
    }
}