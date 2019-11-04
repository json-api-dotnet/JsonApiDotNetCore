using System.Collections;
using System.Collections.Generic;
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

        /// <inheritdoc/>
        public IEnumerable<AttrAttribute> AttributesToSerialize { set; }

        /// <inheritdoc/>
        public IEnumerable<RelationshipAttribute> RelationshipsToSerialize { set; }
    }
}