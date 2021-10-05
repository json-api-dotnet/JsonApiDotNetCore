using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Responsible for getting the set of fields that are to be included for a given type in the serialization result. Typically combines various sources of
    /// information, like application-wide and request-wide sparse fieldsets.
    /// </summary>
    public interface IFieldsToSerialize
    {
        /// <summary>
        /// Indicates whether attributes and relationships should be serialized, based on the current endpoint.
        /// </summary>
        bool ShouldSerialize { get; }

        /// <summary>
        /// Gets the collection of attributes that are to be serialized for resources of type <paramref name="resourceType" />.
        /// </summary>
        IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType);

        /// <summary>
        /// Gets the collection of relationships that are to be serialized for resources of type <paramref name="resourceType" />.
        /// </summary>
        IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceType);
    }
}
