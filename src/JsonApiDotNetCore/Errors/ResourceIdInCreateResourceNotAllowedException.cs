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
        public ResourceIdInCreateResourceNotAllowedException(bool isOperationsRequest, string sourcePointer)
            : base(new ErrorObject(HttpStatusCode.Forbidden)
            {
                Title = isOperationsRequest
                    ? "Specifying the resource ID in operations that create a resource is not allowed."
                    : "Specifying the resource ID in POST resource requests is not allowed.",
                Source = new ErrorSource
                {
                    Pointer = sourcePointer
                }
            })
        {
        }
    }
}
