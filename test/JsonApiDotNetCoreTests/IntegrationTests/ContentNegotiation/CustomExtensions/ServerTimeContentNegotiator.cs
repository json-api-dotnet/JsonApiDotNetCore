using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal sealed class ServerTimeContentNegotiator(IJsonApiOptions options, IHttpContextAccessor httpContextAccessor)
    : JsonApiContentNegotiator(options, httpContextAccessor)
{
    private readonly IJsonApiOptions _options = options;

    protected override IReadOnlyList<JsonApiMediaType> GetPossibleMediaTypes()
    {
        List<JsonApiMediaType> mediaTypes = [];

        if (IsOperationsEndpoint())
        {
            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.AtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.AtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.AtomicOperations) &&
                _options.Extensions.Contains(ServerTimeMediaTypeExtension.ServerTime))
            {
                mediaTypes.Add(ServerTimeMediaTypes.AtomicOperationsWithServerTime);
            }
        }
        else
        {
            mediaTypes.Add(JsonApiMediaType.Default);

            if (_options.Extensions.Contains(ServerTimeMediaTypeExtension.ServerTime))
            {
                mediaTypes.Add(ServerTimeMediaTypes.ServerTime);
            }
        }

        return mediaTypes.AsReadOnly();
    }
}
