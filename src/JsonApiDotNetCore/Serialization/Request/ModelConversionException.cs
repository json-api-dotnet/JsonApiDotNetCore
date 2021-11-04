using System;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Request.Adapters;

namespace JsonApiDotNetCore.Serialization.Request
{
    /// <summary>
    /// The error that is thrown when unable to convert a deserialized request body to an ASP.NET model.
    /// </summary>
    [PublicAPI]
    public sealed class ModelConversionException : Exception
    {
        public string? GenericMessage { get; }
        public string? SpecificMessage { get; }
        public HttpStatusCode? StatusCode { get; }
        public string? SourcePointer { get; }

        public ModelConversionException(RequestAdapterPosition position, string? genericMessage, string? specificMessage, HttpStatusCode? statusCode = null)
            : base(genericMessage)
        {
            ArgumentGuard.NotNull(position, nameof(position));

            GenericMessage = genericMessage;
            SpecificMessage = specificMessage;
            StatusCode = statusCode;
            SourcePointer = position.ToSourcePointer();
        }
    }
}
