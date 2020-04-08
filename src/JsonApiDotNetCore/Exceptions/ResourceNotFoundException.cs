using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when a resource does not exist.
    /// </summary>
    public sealed class ResourceNotFoundException : JsonApiException
    {
        public ResourceNotFoundException(string resourceId, string resourceType) : base(new Error(HttpStatusCode.NotFound)
        {
            Title = "The requested resource does not exist.",
            Detail = $"Resource of type '{resourceType}' with id '{resourceId}' does not exist."
        })
        {
        }
    }
}
