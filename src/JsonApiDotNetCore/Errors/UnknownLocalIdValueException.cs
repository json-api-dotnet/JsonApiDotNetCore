using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when referencing a local ID that hasn't been assigned.
    /// </summary>
    [PublicAPI]
    public sealed class UnknownLocalIdValueException : JsonApiException
    {
        public UnknownLocalIdValueException(string localId)
            : base(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = "Server-generated value for local ID is not available at this point.",
                Detail = $"Server-generated value for local ID '{localId}' is not available at this point."
            })
        {
        }
    }
}
