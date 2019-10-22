using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Contracts
{
    /// <summary>
    /// Responsible for retrieving the exposed resource fields (attributes and
    /// relationships) of registered resources in the resource resourceGraph.
    /// </summary>
    public interface IResourceGraph : IResourceContextProvider
    {
        /// <summary>
        /// Gets all fields (attributes and relationships) for <typeparamref name="TResource"/>
        /// that are targeted by the selector. If no selector is provided, all
        /// exposed fields are returned.
        /// </summary>
        /// <typeparam name="TResource">The resource for which to retrieve fields</typeparam>
        /// <param name="selector">Should be of the form: (TResource e) => new { e.Field1, e.Field2 }</param>
        List<IResourceField> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : IIdentifiable;
        /// <summary>
        /// Gets all attributes for <typeparamref name="TResource"/>
        /// that are targeted by the selector. If no selector is provided, all
        /// exposed fields are returned.
        /// </summary>
        /// <typeparam name="TResource">The resource for which to retrieve attributes</typeparam>
        /// <param name="selector">Should be of the form: (TResource e) => new { e.Attribute1, e.Arttribute2 }</param>
        List<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : IIdentifiable;
        /// <summary>
        /// Gets all relationships for <typeparamref name="TResource"/>
        /// that are targeted by the selector. If no selector is provided, all
        /// exposed fields are returned.
        /// </summary>
        /// <typeparam name="TResource">The resource for which to retrieve relationships</typeparam>
        /// <param name="selector">Should be of the form: (TResource e) => new { e.Relationship1, e.Relationship2 }</param>
        List<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector = null) where TResource : IIdentifiable;
        /// <summary>
        /// Gets all exposed fields (attributes and relationships) for type <paramref name="type"/>
        /// </summary>
        /// <param name="type">The resource type. Must extend IIdentifiable.</param>
        List<IResourceField> GetFields(Type type);
        /// <summary>
        /// Gets all exposed attributes for type <paramref name="type"/>
        /// </summary>
        /// <param name="type">The resource type. Must extend IIdentifiable.</param>
        List<AttrAttribute> GetAttributes(Type type);
        /// <summary>
        /// Gets all exposed relationships for type <paramref name="type"/>
        /// </summary>
        /// <param name="type">The resource type. Must extend IIdentifiable.</param>
        List<RelationshipAttribute> GetRelationships(Type type);

        /// <summary>
        /// Traverses the resource resourceGraph for the inverse relationship of the provided
        /// <paramref name="relationship"/>;
        /// </summary>
        /// <param name="relationship"></param>
        RelationshipAttribute GetInverse(RelationshipAttribute relationship);
    }
}
