using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

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
        private Dictionary<RelationshipAttribute, IIdentifiable> _hasOneRelationships = new Dictionary<RelationshipAttribute, IIdentifiable>();

        /// <summary>
        /// Add the relationship to the list of relationships that should be 
        /// set in the repository layer.
        /// </summary>
        public void Add(RelationshipAttribute relationship, IIdentifiable entity)
            => _hasOneRelationships[relationship] = entity;

        /// <summary>
        /// Get all the models that should be associated
        /// </summary>
        public Dictionary<RelationshipAttribute, IIdentifiable> Get() => _hasOneRelationships;
    }
}
