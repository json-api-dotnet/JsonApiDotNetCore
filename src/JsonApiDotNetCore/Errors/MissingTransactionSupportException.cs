using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when accessing a repository that does not support transactions
    /// during an atomic:operations request.
    /// </summary>
    public sealed class MissingTransactionSupportException : JsonApiException
    {
        public MissingTransactionSupportException(string resourceType)
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Unsupported resource type in atomic:operations request.",
                Detail = $"Operations on resources of type '{resourceType}' cannot be used because transaction support is unavailable."
            })
        {
        }
    }
}
