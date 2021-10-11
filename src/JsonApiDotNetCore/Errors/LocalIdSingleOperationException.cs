#nullable disable

using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when assigning and referencing a local ID within the same operation.
    /// </summary>
    [PublicAPI]
    public sealed class LocalIdSingleOperationException : JsonApiException
    {
        public LocalIdSingleOperationException(string localId)
            : base(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = "Local ID cannot be both defined and used within the same operation.",
                Detail = $"Local ID '{localId}' cannot be both defined and used within the same operation."
            })
        {
        }
    }
}
