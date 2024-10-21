using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Performs content negotiation for JSON:API requests.
/// </summary>
public interface IJsonApiContentNegotiator
{
    /// <summary>
    /// Validates the Content-Type and Accept HTTP headers from the incoming request. Throws a <see cref="JsonApiException" /> if unsupported. Otherwise,
    /// returns the list of negotiated JSON:API extensions, which should always be a subset of <see cref="IJsonApiOptions.Extensions" />.
    /// </summary>
    IReadOnlySet<JsonApiExtension> Negotiate();
}
