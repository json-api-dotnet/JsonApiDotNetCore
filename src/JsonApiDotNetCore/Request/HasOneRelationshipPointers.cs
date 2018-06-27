using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Request
{
    /// <summary>
    /// Stores information to set relationships for the request resource. 
    /// These relationships must already exist and should not be re-created.
    /// 
    /// The expected use case is POST-ing or PATCH-ing 
    /// an entity with HasOne relationships:
    /// <code>
    /// {
    ///    "data": {
    ///      "type": "photos",
    ///      "attributes": {
    ///        "title": "Ember Hamster",
    ///        "src": "http://example.com/images/productivity.png"
    ///      },
    ///      "relationships": {
    ///        "photographer": {
    ///          "data": { "type": "people", "id": "2" }
    ///        }
    ///      }
    ///    }
    ///  }
    /// </code>
    /// </summary>
    public class HasOneRelationshipPointers
    {
        private Dictionary<Type, IIdentifiable> _hasOneRelationships = new Dictionary<Type, IIdentifiable>();

        /// <summary>
        /// Add the relationship to the list of relationships that should be 
        /// set in the repository layer.
        /// </summary>
        public void Add(Type dependentType, IIdentifiable entity)
            => _hasOneRelationships[dependentType] = entity;

        /// <summary>
        /// Get all the models that should be associated
        /// </summary>
        public Dictionary<Type, IIdentifiable> Get() => _hasOneRelationships;
    }
}
