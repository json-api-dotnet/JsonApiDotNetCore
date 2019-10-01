using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <summary>
    /// Abstract base class for serialization that extends <see cref="ResourceObjectBuilder"/>.
    /// Converts entities in to <see cref="ResourceObject"/>s and wraps them in a <see cref="Document"/>.
    /// </summary>
    public abstract class DocumentBuilder : ResourceObjectBuilder
    {
        protected DocumentBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider, ISerializerBehaviourProvider behaviourProvider) : base(resourceGraph, provider, behaviourProvider) { }

        /// <summary>
        /// Builds a <see cref="Document"/> for <paramref name="entity"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="entity">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        protected Document Build(IIdentifiable entity, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
        {
            if (entity == null)
                return new Document();

            return new Document { Data = BuildResourceObject(entity, attributes, relationships) };
        }

        /// <summary>
        /// Builds a <see cref="Document"/> for <paramref name="entities"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="entities">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        protected Document Build(IEnumerable entities, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
        {
            var data = new List<ResourceObject>();
            foreach (IIdentifiable entity in entities)
                data.Add(BuildResourceObject(entity, attributes, relationships));

            return new Document { Data = data };
        }
    }
}
