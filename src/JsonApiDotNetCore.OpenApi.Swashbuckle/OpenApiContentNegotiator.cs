using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class OpenApiContentNegotiator(IJsonApiOptions options, IHttpContextAccessor httpContextAccessor)
    : JsonApiContentNegotiator(options, httpContextAccessor)
{
    private readonly IJsonApiOptions _options = options;

    protected override JsonApiMediaType? GetDefaultMediaType(IReadOnlyList<JsonApiMediaType> possibleMediaTypes, JsonApiMediaType? requestMediaType)
    {
        if (requestMediaType != null && possibleMediaTypes.Contains(requestMediaType))
        {
            // Bug workaround: NSwag doesn't send an Accept header when only non-success responses define a Content-Type.
            // This occurs on POST/PATCH/DELETE at a JSON:API relationships endpoint.
            return requestMediaType;
        }

        return base.GetDefaultMediaType(possibleMediaTypes, requestMediaType);
    }

    protected override IReadOnlyList<JsonApiMediaType> GetPossibleMediaTypes()
    {
        List<JsonApiMediaType> mediaTypes = [];

        // JSON:API compliant entries come after relaxed entries, which makes them less likely to be selected.
        // This improves compatibility with client generators, which often generate broken code due to the double quotes.

        if (IsOperationsEndpoint())
        {
            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.RelaxedAtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedAtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.AtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.AtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.RelaxedAtomicOperations) &&
                _options.Extensions.Contains(OpenApiMediaTypeExtension.RelaxedOpenApi))
            {
                mediaTypes.Add(OpenApiMediaTypes.RelaxedAtomicOperationsWithRelaxedOpenApi);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.AtomicOperations) && _options.Extensions.Contains(OpenApiMediaTypeExtension.OpenApi))
            {
                mediaTypes.Add(OpenApiMediaTypes.AtomicOperationsWithOpenApi);
            }
        }
        else
        {
            if (_options.Extensions.Contains(OpenApiMediaTypeExtension.RelaxedOpenApi))
            {
                mediaTypes.Add(OpenApiMediaTypes.RelaxedOpenApi);
            }

            if (_options.Extensions.Contains(OpenApiMediaTypeExtension.OpenApi))
            {
                mediaTypes.Add(OpenApiMediaTypes.OpenApi);
            }

            mediaTypes.Add(JsonApiMediaType.Default);
        }

        return mediaTypes.AsReadOnly();
    }
}
