using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// Basically a enumerable of <typeparamref name="TResource" /> of resources that were affected by the request. Also contains information about updated
    /// relationships through implementation of IAffectedRelationshipsDictionary<typeparamref name="TResource" />>
    /// </summary>
    public interface IResourceHashSet<TResource> : IByAffectedRelationships<TResource>, IReadOnlyCollection<TResource>
        where TResource : class, IIdentifiable
    {
    }
}
