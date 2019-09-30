using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    public interface ISerializableFields
    {
        /// <summary>
        /// Gets the list of attributes that are allowed to be serialized for
        /// resource of type <paramref name="type"/>
        /// </summary>
        List<AttrAttribute> GetAllowedAttributes(Type type);
        /// <summary>
        /// Gets the list of relationships that are allowed to be serialized for
        /// resource of type <paramref name="type"/>
        /// </summary>
        List<RelationshipAttribute> GetAllowedRelationships(Type type);
    }
}
