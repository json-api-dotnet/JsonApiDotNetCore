using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Responsible for converting entities in to <see cref="ResourceObject"/>s
    /// given a list of attributes and relationships.
    /// </summary> 
    public interface IResourceObjectBuilder
    {
        /// <summary>
        /// Converts <paramref name="entity"/> into a <see cref="ResourceObject"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="entity">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        ResourceObject Build(IIdentifiable entity, IEnumerable<AttrAttribute> attributes, IEnumerable<RelationshipAttribute> relationships);
    }
}
