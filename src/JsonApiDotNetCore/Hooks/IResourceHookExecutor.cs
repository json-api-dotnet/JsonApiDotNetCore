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
    /// </summary>^
    public interface IResourceHookExecutor : IBeforeExecutor, IAfterExecutor, IOnExecutor { }

    public interface IBeforeExecutor
    {
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="EntityResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeCreate"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        IEnumerable<TEntity> BeforeCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeRead"/>
        /// hook where T = <typeparamref name="TEntity"/> for the requested 
        /// entities as well as any related relationship.
        /// </summary>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <param name="stringId">StringId of the requested entity in the case of 
        /// <see cref="EntityResourceService{X, Y, Z}.GetAsync(Z)"/>.</param>
        /// <typeparam name="TEntity">The type of the request entity</typeparam>
        void BeforeRead<TEntity>(ResourceAction pipeline, string stringId = null) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="EntityResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeUpdate"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
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
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        IEnumerable<TEntity> BeforeUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the Before Cycle by firing the appropiate hooks if they are implemented. 
        /// The returned set will be used in the actual operation in <see cref="EntityResourceService{T}"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.BeforeDelete"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.BeforeImplicitUpdateRelationship"/>
        /// hook for any entities that are indirectly (implicitly) affected by this operation.
        /// Eg: when deleting an entity that has relationships set to other entities, 
        /// these other entities are implicitly affected by the delete operation.
        /// </summary>
        /// <returns>The transformed set</returns>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        IEnumerable<TEntity> BeforeDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
    }

    public interface IAfterExecutor
    {
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterCreate"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.AfterUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        void AfterCreate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterRead"/> for every unique
        /// entity type occuring in parameter <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        void AfterRead<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterUpdate"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
        /// <para />
        /// Fires the <see cref="ResourceDefinition{U}.AfterUpdateRelationship"/>
        /// hook for any related (nested) entity for values within parameter <paramref name="entities"/>
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        void AfterUpdate<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
        /// <summary>
        /// Executes the After Cycle by firing the appropiate hooks if they are implemented. 
        /// <para />
        /// Fires the <see cref="ResourceDefinition{T}.AfterDelete"/>
        /// hook where T = <typeparamref name="TEntity"/> for values in parameter <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">Target entities for the Before cycle.</param>
        /// <param name="pipeline">An enum indicating from where the hook was triggered.</param>
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        void AfterDelete<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline, bool succeeded) where TEntity : class, IIdentifiable;
    }

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
        /// <typeparam name="TEntity">The type of the root entities</typeparam>
        IEnumerable<TEntity> OnReturn<TEntity>(IEnumerable<TEntity> entities, ResourceAction pipeline) where TEntity : class, IIdentifiable;
    }
}