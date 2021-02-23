using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// A dummy interface used internally by the hook executor.
    /// </summary>
    public interface IRelationshipsDictionary
    {
    }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which resources.
    /// </summary>
    public interface IRelationshipsDictionary<TRightResource>
        : IRelationshipGetters<TRightResource>, IReadOnlyDictionary<RelationshipAttribute, HashSet<TRightResource>>, IRelationshipsDictionary
        where TRightResource : class, IIdentifiable
    {
    }
}
