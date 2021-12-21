using System.Net;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Middleware;

[PublicAPI]
public sealed class IdempotentResponse
{
    public string RequestFingerprint { get; }

    public HttpStatusCode ResponseStatusCode { get; }
    public string? ResponseLocationHeader { get; }
    public string? ResponseContentTypeHeader { get; }
    public string? ResponseBody { get; }

    public IdempotentResponse(string requestFingerprint, HttpStatusCode responseStatusCode, string? responseLocationHeader, string? responseContentTypeHeader,
        string? responseBody)
    {
        RequestFingerprint = requestFingerprint;
        ResponseStatusCode = responseStatusCode;
        ResponseLocationHeader = responseLocationHeader;
        ResponseContentTypeHeader = responseContentTypeHeader;
        ResponseBody = responseBody;
    }
}
