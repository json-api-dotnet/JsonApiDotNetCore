using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// An interface that is implemented to expose a relationship dictionary on another class.
    /// </summary>
    public interface IByAffectedRelationships<TRightResource> : IRelationshipGetters<TRightResource>
        where TRightResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of affected resources grouped by affected relationships.
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TRightResource>> AffectedRelationships { get; }
    }
}
