using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Serialization.Server
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
        /// if <paramref name="relationship"/>, it will consider the allowed list of attributes
        /// as an included relationship
        /// </summary>
        List<AttrAttribute> GetAllowedAttributes(Type type, RelationshipAttribute relationship = null);
        /// <summary>
        /// Gets the list of relationships that are allowed to be serialized for
        /// resource of type <paramref name="type"/>
        /// </summary>
        List<RelationshipAttribute> GetAllowedRelationships(Type type);
    }
}
