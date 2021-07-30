using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Enables retrieving the exposed resource fields (attributes and relationships) of resources registered in the resource graph.
    /// </summary>
    [PublicAPI]
    public interface IResourceGraph : IResourceContextProvider
    {
        /// <summary>
        /// Gets all fields (attributes and relationships) for <typeparamref name="TResource" /> that are targeted by the selector. If no selector is provided,
        /// all exposed fields are returned.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource type for which to retrieve fields.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource r) => new { r.Field1, r.Field2 }
        /// </param>
        IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Gets all attributes for <typeparamref name="TResource" /> that are targeted by the selector. If no selector is provided, all exposed attributes are
        /// returned.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource type for which to retrieve attributes.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource r) => new { r.Attribute1, r.Attribute2 }
        /// </param>
        IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Gets all relationships for <typeparamref name="TResource" /> that are targeted by the selector. If no selector is provided, all exposed relationships
        /// are returned.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource type for which to retrieve relationships.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource r) => new { r.Relationship1, r.Relationship2 }
        /// </param>
        IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector)
            where TResource : class, IIdentifiable;
    }
}
