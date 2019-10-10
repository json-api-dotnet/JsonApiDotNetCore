using System.Collections;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// <summary>
    /// Interface for client serializer that can be used to register with the DI, for usage in
    /// custom services or repositories.
    /// </summary>
    public interface IRequestSerializer
    {
        /// <summary>
        /// Creates and serializes a document for a single intance of a resource.
        /// </summary>
        /// <param name="entity">Entity to serialize</param>
        /// <returns>The serialized content</returns>
        string Serialize(IIdentifiable entity);
        /// <summary>
        /// Creates and serializes a document for for a list of entities of one resource.
        /// </summary>
        /// <param name="entities">Entities to serialize</param>
        /// <returns>The serialized content</returns>
        string Serialize(IEnumerable entities);
        /// <summary>
        /// Sets the <see cref="AttrAttribute"/>s to serialize for resources of type <typeparamref name="TResource"/>.
        /// If no <see cref="AttrAttribute"/>s are specified, by default all attributes are included in the serialized result.
        /// </summary>
        /// <typeparam name="TResource">Type of the resource to serialize</typeparam>
        /// <param name="filter">Should be of the form: (TResource e) => new { e.Attr1, e.Attr2 }</param>
        void SetAttributesToSerialize<TResource>(Expression<System.Func<TResource, dynamic>> filter) where TResource : class, IIdentifiable;
        /// <summary>
        /// Sets the <see cref="RelationshipAttribute"/>s to serialize for resources of type <typeparamref name="TResource"/>.
        /// If no <see cref="RelationshipAttribute"/>s are specified, by default no relationships are included in the serialization result.
        /// The <paramref name="filter"/>should be of the form: (TResource e) => new { e.Attr1, e.Attr2 }
        /// </summary>
        /// <typeparam name="TResource">Type of the resource to serialize</typeparam>
        /// <param name="filter">Should be of the form: (TResource e) => new { e.Attr1, e.Attr2 }</param>
        void SetRelationshipsToSerialize<TResource>(Expression<System.Func<TResource, dynamic>> filter) where TResource : class, IIdentifiable;
    }
}