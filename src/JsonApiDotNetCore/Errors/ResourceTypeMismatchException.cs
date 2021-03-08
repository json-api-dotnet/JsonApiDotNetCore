using System.Net;
using System.Net.Http;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when the resource type in the request body does not match the type expected at the current endpoint URL.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceTypeMismatchException : JsonApiException
    {
        public ResourceTypeMismatchException(HttpMethod method, string requestPath, ResourceContext expected, ResourceContext actual)
            : base(new Error(HttpStatusCode.Conflict)
            {
                Title = "Resource type mismatch between request body and endpoint URL.",
                Detail = $"Expected resource of type '{expected.PublicName}' in {method} " +
                    $"request body at endpoint '{requestPath}', instead of '{actual?.PublicName}'."
            })
        {
        }
    }
}
