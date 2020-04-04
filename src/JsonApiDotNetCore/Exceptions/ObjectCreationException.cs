using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown of resource object creation fails.
    /// </summary>
    public sealed class ObjectCreationException : JsonApiException
    {
        public Type Type { get; }

        public ObjectCreationException(Type type, Exception innerException)
            : base(new Error(HttpStatusCode.InternalServerError)
            {
                Title = "Failed to create an object instance using its default constructor.",
                Detail = $"Failed to create an instance of '{type.FullName}' using its default constructor."
            }, innerException)
        {
            Type = type;
        }
    }
}
