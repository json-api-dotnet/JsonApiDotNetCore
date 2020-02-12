using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// <summary>
    /// Interface for client serializer that can be used to register with the DI, for usage in
    /// custom services or repositories.
    /// </summary>
    public interface IRequestSerializer
    {
        /// <summary>
        /// Creates and serializes a document for a single instance of a resource.
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
        /// Sets the attributes that will be included in the serialized payload.
        /// You can use <see cref="IResourceGraph.GetAttributes{TResource}(Expression{System.Func{TResource, dynamic}})"/>
        /// to conveniently access the desired <see cref="AttrAttribute"/> instances
        /// </summary>
        public IEnumerable<AttrAttribute> AttributesToSerialize { set; }
        /// <summary>
        /// Sets the relationships that will be included in the serialized payload.
        /// You can use <see cref="IResourceGraph.GetRelationships{TResource}(Expression{System.Func{TResource, dynamic}})"/>
        /// to conveniently access the desired <see cref="RelationshipAttribute"/> instances
        /// </summary>
        public IEnumerable<RelationshipAttribute> RelationshipsToSerialize { set; }
    }
}
