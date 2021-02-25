using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a resource creation request or operation is received that contains a client-generated ID.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceIdInCreateResourceNotAllowedException : JsonApiException
    {
        public ResourceIdInCreateResourceNotAllowedException(int? atomicOperationIndex = null)
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = atomicOperationIndex == null
                    ? "Specifying the resource ID in POST requests is not allowed."
                    : "Specifying the resource ID in operations that create a resource is not allowed.",
                Source =
                {
                    Pointer = atomicOperationIndex != null ? $"/atomic:operations[{atomicOperationIndex}]/data/id" : "/data/id"
                }
            })
        {
        }
    }
}
