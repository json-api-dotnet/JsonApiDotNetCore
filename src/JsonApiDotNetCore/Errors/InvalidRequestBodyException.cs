using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when deserializing the request body fails.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidRequestBodyException : JsonApiException
    {
        public InvalidRequestBodyException(string requestBody, string genericMessage, string specificMessage, string sourcePointer,
            HttpStatusCode? alternativeStatusCode = null, Exception innerException = null)
            : base(new ErrorObject(alternativeStatusCode ?? HttpStatusCode.UnprocessableEntity)
            {
                Title = genericMessage != null ? $"Failed to deserialize request body: {genericMessage}" : "Failed to deserialize request body.",
                Detail = specificMessage,
                Source = sourcePointer == null
                    ? null
                    : new ErrorSource
                    {
                        Pointer = sourcePointer
                    },
                Meta = string.IsNullOrEmpty(requestBody)
                    ? null
                    : new Dictionary<string, object>
                    {
                        ["RequestBody"] = requestBody
                    }
            }, innerException)
        {
        }
    }
}
