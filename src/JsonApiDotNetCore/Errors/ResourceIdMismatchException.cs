using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when the resource ID in the request body does not match the ID in the current endpoint URL.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceIdMismatchException : JsonApiException
    {
        public ResourceIdMismatchException(string bodyId, string endpointId, string requestPath)
            : base(new Error(HttpStatusCode.Conflict)
            {
                Title = "Resource ID mismatch between request body and endpoint URL.",
                Detail = $"Expected resource ID '{endpointId}' in PATCH request body " + $"at endpoint '{requestPath}', instead of '{bodyId}'."
            })
        {
        }
    }
}
