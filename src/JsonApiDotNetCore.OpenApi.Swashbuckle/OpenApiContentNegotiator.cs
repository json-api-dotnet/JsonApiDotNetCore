using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class OpenApiContentNegotiator(IJsonApiOptions options, IHttpContextAccessor httpContextAccessor)
    : JsonApiContentNegotiator(options, httpContextAccessor)
{
    private readonly IJsonApiOptions _options = options;

    protected override IReadOnlyList<JsonApiMediaType> GetPossibleMediaTypes()
    {
        List<JsonApiMediaType> mediaTypes = [];

        // JSON:API compliant entries come after relaxed entries, which makes them less likely to be selected.
        // This improves compatibility with client generators, which often generate broken code due to the double quotes.

        if (IsOperationsEndpoint())
        {
            if (_options.Extensions.Contains(JsonApiExtension.RelaxedAtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedAtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiExtension.AtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.AtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiExtension.RelaxedAtomicOperations) && _options.Extensions.Contains(JsonApiExtension.RelaxedOpenApi))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedAtomicOperationsWithRelaxedOpenApi);
            }

            if (_options.Extensions.Contains(JsonApiExtension.AtomicOperations) && _options.Extensions.Contains(JsonApiExtension.OpenApi))
            {
                mediaTypes.Add(JsonApiMediaType.AtomicOperationsWithOpenApi);
            }
        }
        else
        {
            if (_options.Extensions.Contains(JsonApiExtension.RelaxedOpenApi))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedOpenApi);
            }

            if (_options.Extensions.Contains(JsonApiExtension.OpenApi))
            {
                mediaTypes.Add(JsonApiMediaType.OpenApi);
            }

            mediaTypes.Add(JsonApiMediaType.Default);
        }

        return mediaTypes.AsReadOnly();
    }
}
