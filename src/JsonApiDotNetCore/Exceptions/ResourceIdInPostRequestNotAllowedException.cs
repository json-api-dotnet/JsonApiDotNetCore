using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when a POST request is received that contains a client-generated ID.
    /// </summary>
    public sealed class ResourceIdInPostRequestNotAllowedException : JsonApiException
    {
        public ResourceIdInPostRequestNotAllowedException()
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "Specifying the resource id in POST requests is not allowed.",
                Source =
                {
                    Pointer = "/data/id"
                }
            })
        {
        }
    }
}
