using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// A wrapper class that contains information about the resources that are updated by the request. Contains the resources from the request and the
    /// corresponding database values. Also contains information about updated relationships through implementation of IRelationshipsDictionary
    /// <typeparamref name="TResource" />>
    /// </summary>
    public interface IDiffableResourceHashSet<TResource> : IResourceHashSet<TResource>
        where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Iterates over diffs, which is the affected resource from the request with their associated current value from the database.
        /// </summary>
        IEnumerable<ResourceDiffPair<TResource>> GetDiffs();
    }
}
