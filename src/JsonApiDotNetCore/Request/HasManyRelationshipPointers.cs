using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Request
{
    /// <summary>
    /// Stores information to set relationships for the request resource. 
    /// These relationships must already exist and should not be re-created.
    /// 
    /// The expected use case is POST-ing or PATCH-ing 
    /// an entity with HasMany relaitonships:
    /// <code>
    /// {
    ///    "data": {
    ///      "type": "photos",
    ///      "attributes": {
    ///        "title": "Ember Hamster",
    ///        "src": "http://example.com/images/productivity.png"
    ///      },
    ///      "relationships": {
    ///        "tags": {
    ///          "data": [
    ///            { "type": "tags", "id": "2" },
    ///            { "type": "tags", "id": "3" }
    ///          ]
    ///        }
    ///      }
    ///    }
    ///  }
    /// </code>
    /// </summary>
    public class HasManyRelationshipPointers
    {
        private Dictionary<Type, IList> _hasManyRelationships = new Dictionary<Type, IList>();

        /// <summary>
        /// Add the relationship to the list of relationships that should be 
        /// set in the repository layer.
        /// </summary>
        public void Add(Type dependentType, IList entities)
            => _hasManyRelationships[dependentType] = entities;

        /// <summary>
        /// Get all the models that should be associated
        /// </summary>
        public Dictionary<Type, IList> Get() => _hasManyRelationships;
    }
}
