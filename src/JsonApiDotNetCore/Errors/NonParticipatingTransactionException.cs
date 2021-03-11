using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a repository does not participate in the overarching transaction during an atomic:operations request.
    /// </summary>
    [PublicAPI]
    public sealed class NonParticipatingTransactionException : JsonApiException
    {
        public NonParticipatingTransactionException()
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Unsupported combination of resource types in atomic:operations request.",
                Detail = "All operations need to participate in a single shared transaction, " + "which is not the case for this request."
            })
        {
        }
    }
}
