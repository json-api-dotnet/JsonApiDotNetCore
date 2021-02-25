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
        /// The resource for which to retrieve fields.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource e) => new { e.Field1, e.Field2 }
        /// </param>
        IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Gets all attributes for <typeparamref name="TResource" /> that are targeted by the selector. If no selector is provided, all exposed fields are
        /// returned.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource for which to retrieve attributes.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource e) => new { e.Attribute1, e.Attribute2 }
        /// </param>
        IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Gets all relationships for <typeparamref name="TResource" /> that are targeted by the selector. If no selector is provided, all exposed fields are
        /// returned.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource for which to retrieve relationships.
        /// </typeparam>
        /// <param name="selector">
        /// Should be of the form: (TResource e) => new { e.Relationship1, e.Relationship2 }
        /// </param>
        IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, dynamic>> selector = null)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Gets all exposed fields (attributes and relationships) for the specified type.
        /// </summary>
        /// <param name="type">
        /// The resource type. Must implement <see cref="IIdentifiable" />.
        /// </param>
        IReadOnlyCollection<ResourceFieldAttribute> GetFields(Type type);

        /// <summary>
        /// Gets all exposed attributes for the specified type.
        /// </summary>
        /// <param name="type">
        /// The resource type. Must implement <see cref="IIdentifiable" />.
        /// </param>
        IReadOnlyCollection<AttrAttribute> GetAttributes(Type type);

        /// <summary>
        /// Gets all exposed relationships for the specified type.
        /// </summary>
        /// <param name="type">
        /// The resource type. Must implement <see cref="IIdentifiable" />.
        /// </param>
        IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type type);

        /// <summary>
        /// Traverses the resource graph, looking for the inverse relationship of the specified <paramref name="relationship" />.
        /// </summary>
        RelationshipAttribute GetInverseRelationship(RelationshipAttribute relationship);
    }
}
