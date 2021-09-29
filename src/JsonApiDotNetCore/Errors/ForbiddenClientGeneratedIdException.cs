using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when creating a resource with a client-generated ID.
    /// </summary>
    [PublicAPI]
    public sealed class ForbiddenClientGeneratedIdException : JsonApiException
    {
        public ForbiddenClientGeneratedIdException(string sourcePointer)
            : base(new ErrorObject(HttpStatusCode.Forbidden)
            {
                Title = "The use of client-generated IDs is disabled.",
                Source = new ErrorSource
                {
                    Pointer = sourcePointer
                }
            })
        {
        }
    }
}
