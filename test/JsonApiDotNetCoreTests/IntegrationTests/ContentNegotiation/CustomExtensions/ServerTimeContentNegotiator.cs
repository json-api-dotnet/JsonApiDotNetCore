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

        // Relaxed entries come after JSON:API compliant entries, which makes them less likely to be selected.

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

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.RelaxedAtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedAtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.RelaxedAtomicOperations) &&
                _options.Extensions.Contains(ServerTimeMediaTypeExtension.RelaxedServerTime))
            {
                mediaTypes.Add(ServerTimeMediaTypes.RelaxedAtomicOperationsWithRelaxedServerTime);
            }
        }
        else
        {
            mediaTypes.Add(JsonApiMediaType.Default);

            if (_options.Extensions.Contains(ServerTimeMediaTypeExtension.ServerTime))
            {
                mediaTypes.Add(ServerTimeMediaTypes.ServerTime);
            }

            if (_options.Extensions.Contains(ServerTimeMediaTypeExtension.RelaxedServerTime))
            {
                mediaTypes.Add(ServerTimeMediaTypes.RelaxedServerTime);
            }
        }

        return mediaTypes.AsReadOnly();
    }
}
