using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Relationship group entry.
    /// </summary>
    public class RelationshipGroupEntry
    {
        public RelationshipProxy Relationship { get; private set; }
        public HashSet<IIdentifiable> Entities { get; private set; }
        public RelationshipGroupEntry(RelationshipProxy relationship, HashSet<IIdentifiable> entities)
        {
            Relationship = relationship;
            Entities = entities;
        }

    }
}

