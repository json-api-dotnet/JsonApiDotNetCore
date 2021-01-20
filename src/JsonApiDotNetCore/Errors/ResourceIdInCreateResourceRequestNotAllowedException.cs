using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a resource creation request is received that contains a client-generated ID.
    /// </summary>
    public sealed class ResourceIdInCreateResourceRequestNotAllowedException : JsonApiException
    {
        public ResourceIdInCreateResourceRequestNotAllowedException(int? atomicOperationIndex = null)
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = atomicOperationIndex == null
                    ? "Specifying the resource ID in POST requests is not allowed."
                    : "Specifying the resource ID in operations that create a resource is not allowed.",
                Source =
                {
                    Pointer = atomicOperationIndex != null
                        ? $"/atomic:operations[{atomicOperationIndex}]/data/id"
                        : "/data/id"
                }
            })
        {
        }
    }
}
