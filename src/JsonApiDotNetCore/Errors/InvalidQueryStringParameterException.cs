using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when processing the request fails due to an error in the request query string.
/// </summary>
[PublicAPI]
public sealed class InvalidQueryStringParameterException(string parameterName, string genericMessage, string specificMessage, Exception? innerException = null)
    : JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
    {
        Title = genericMessage,
        Detail = specificMessage,
        Source = new ErrorSource
        {
            Parameter = parameterName
        }
    }, innerException)
{
    public string ParameterName { get; } = parameterName;
}
