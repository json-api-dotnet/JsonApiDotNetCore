using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when trying to change the ID of an existing resource.
    /// </summary>
    public sealed class ResourceIdIsReadOnlyException : JsonApiException
    {
        public ResourceIdIsReadOnlyException()
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "Resource ID is read-only.",
            })
        {
        }
    }
}
