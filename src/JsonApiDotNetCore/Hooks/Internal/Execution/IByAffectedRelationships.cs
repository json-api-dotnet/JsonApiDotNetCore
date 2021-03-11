using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// An interface that is implemented to expose a relationship dictionary on another class.
    /// </summary>
    [PublicAPI]
    public interface IByAffectedRelationships<TRightResource> : IRelationshipGetters<TRightResource>
        where TRightResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of affected resources grouped by affected relationships.
        /// </summary>
        IDictionary<RelationshipAttribute, HashSet<TRightResource>> AffectedRelationships { get; }
    }
}
