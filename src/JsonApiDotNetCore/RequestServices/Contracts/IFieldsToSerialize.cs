using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Models
{
    /// <summary>
    /// Responsible for getting the set of fields that are to be included for a
    /// given type in the serialization result. Typically combines various sources
    /// of information, like application-wide hidden fields as set in
    /// <see cref="ResourceDefinition{TResource}"/>, or request-wide hidden fields
    /// through sparse field selection.
    /// </summary>
    public interface IFieldsToSerialize
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
