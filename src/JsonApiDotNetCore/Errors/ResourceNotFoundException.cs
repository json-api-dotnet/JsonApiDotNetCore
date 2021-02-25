using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a resource does not exist.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceNotFoundException : JsonApiException
    {
        public ResourceNotFoundException(string resourceId, string resourceType)
            : base(new Error(HttpStatusCode.NotFound)
            {
                Title = "The requested resource does not exist.",
                Detail = $"Resource of type '{resourceType}' with ID '{resourceId}' does not exist."
            })
        {
        }
    }
}
