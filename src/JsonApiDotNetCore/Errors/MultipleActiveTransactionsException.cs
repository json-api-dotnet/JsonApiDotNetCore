using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a repository does not participate in the overarching transaction
    /// during an atomic:operations request.
    /// </summary>
    public sealed class MultipleActiveTransactionsException : JsonApiException
    {
        public MultipleActiveTransactionsException()
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Unsupported combination of resource types in atomic:operations request.",
                Detail = "All operations need to participate in a single shared transaction, which is not the case for this request."
            })
        {
        }
    }
}
