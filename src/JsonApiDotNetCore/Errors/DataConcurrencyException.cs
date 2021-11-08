using System;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when data has been modified on the server since the resource was retrieved.
    /// </summary>
    [PublicAPI]
    public sealed class DataConcurrencyException : JsonApiException
    {
        public DataConcurrencyException(Exception exception)
            : base(new ErrorObject(HttpStatusCode.Conflict)
            {
                Title = "The concurrency token is missing or does not match the server version. " +
                    "This indicates that data has been modified since the resource was retrieved."
            }, exception)
        {
        }
    }
}
