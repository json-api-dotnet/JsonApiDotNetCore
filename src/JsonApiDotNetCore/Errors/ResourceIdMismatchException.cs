using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when the resource id in the request body does not match the id in the current endpoint URL.
    /// </summary>
    public sealed class ResourceIdMismatchException : JsonApiException
    {
        public ResourceIdMismatchException(string bodyId, string endpointId, string requestPath)
            : base(new Error(HttpStatusCode.Conflict)
            {
                Title = "Resource id mismatch between request body and endpoint URL.",
                Detail = $"Expected resource id '{endpointId}' in PATCH request body at endpoint '{requestPath}', instead of '{bodyId}'."
            })
        {
        }
    }
}
