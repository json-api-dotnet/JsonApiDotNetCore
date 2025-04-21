using System.Globalization;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Response;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal sealed class ServerTimeResponseMeta(IJsonApiRequest request, RequestDocumentStore documentStore, TimeProvider timeProvider) : IResponseMeta
{
    private readonly IJsonApiRequest _request = request;
    private readonly RequestDocumentStore _documentStore = documentStore;
    private readonly TimeProvider _timeProvider = timeProvider;

    public IDictionary<string, object?>? GetMeta()
    {
        if (_request.Extensions.Contains(ServerTimeMediaTypeExtension.ServerTime) ||
            _request.Extensions.Contains(ServerTimeMediaTypeExtension.RelaxedServerTime))
        {
            if (_documentStore.Document is not { Meta: not null } || !_documentStore.Document.Meta.TryGetValue("useLocalTime", out object? useLocalTimeValue) ||
                useLocalTimeValue == null || !bool.TryParse(useLocalTimeValue.ToString(), out bool useLocalTime))
            {
                useLocalTime = false;
            }

            return useLocalTime
                ? new Dictionary<string, object?>
                {
                    ["localServerTime"] = _timeProvider.GetLocalNow().ToString("O", CultureInfo.InvariantCulture)
                }
                : new Dictionary<string, object?>
                {
                    ["utcServerTime"] = _timeProvider.GetUtcNow().UtcDateTime.ToString("O", CultureInfo.InvariantCulture)
                };
        }

        return null;
    }
}
