using System.Threading;
using JetBrains.Annotations;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// An enum that represents the initiator of a resource hook. Eg, when BeforeCreate() is called from
    /// <see cref="JsonApiResourceService{TResource,TId}.GetAsync(TId, CancellationToken)" />, it will be called with parameter pipeline =
    /// ResourceAction.GetSingle.
    /// </summary>
    [PublicAPI]
    public enum ResourcePipeline
    {
        None,
        Get,
        GetSingle,
        GetRelationship,
        Post,
        Patch,
        PatchRelationship,
        Delete
    }
}
