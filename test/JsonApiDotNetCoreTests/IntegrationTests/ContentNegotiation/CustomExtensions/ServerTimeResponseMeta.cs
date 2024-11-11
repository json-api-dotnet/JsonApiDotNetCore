using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Response;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal sealed class ServerTimeResponseMeta(IJsonApiRequest request, RequestDocumentStore documentStore) : IResponseMeta
{
    private readonly RequestDocumentStore _documentStore = documentStore;

    public IDictionary<string, object?>? GetMeta()
    {
        if (request.Extensions.Contains(ServerTimeMediaTypeExtension.ServerTime) || request.Extensions.Contains(ServerTimeMediaTypeExtension.RelaxedServerTime))
        {
            if (_documentStore.Document is not { Meta: not null } || !_documentStore.Document.Meta.TryGetValue("useLocalTime", out object? useLocalTimeValue) ||
                useLocalTimeValue == null || !bool.TryParse(useLocalTimeValue.ToString(), out bool useLocalTime))
            {
                useLocalTime = false;
            }

            return useLocalTime
                ? new Dictionary<string, object?>
                {
                    ["localServerTime"] = DateTime.Now.ToString("O")
                }
                : new Dictionary<string, object?>
                {
                    ["utcServerTime"] = DateTime.UtcNow.ToString("O")
                };
        }

        return null;
    }
}
