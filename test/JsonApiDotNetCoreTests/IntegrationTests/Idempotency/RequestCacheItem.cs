using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[NoResource]
public sealed class RequestCacheItem
{
    public string Id { get; set; }
    public string RequestFingerprint { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public HttpStatusCode? ResponseStatusCode { get; set; }
    public string? ResponseLocationHeader { get; set; }
    public string? ResponseContentTypeHeader { get; set; }
    public string? ResponseBody { get; set; }

    public RequestCacheItem(string id, string requestFingerprint, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        RequestFingerprint = requestFingerprint;
    }
}
