using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly IJsonApiRoutingConvention _jsonApiRoutingConvention;
    private readonly OpenApiEndpointConvention _openApiEndpointConvention;
    private readonly JsonApiRequestFormatMetadataProvider _jsonApiRequestFormatMetadataProvider;
    private readonly IJsonApiOptions _jsonApiOptions;

    public ConfigureMvcOptions(IJsonApiRoutingConvention jsonApiRoutingConvention, OpenApiEndpointConvention openApiEndpointConvention,
        JsonApiRequestFormatMetadataProvider jsonApiRequestFormatMetadataProvider, IJsonApiOptions jsonApiOptions)
    {
        ArgumentGuard.NotNull(jsonApiRoutingConvention);
        ArgumentGuard.NotNull(openApiEndpointConvention);
        ArgumentGuard.NotNull(jsonApiRequestFormatMetadataProvider);
        ArgumentGuard.NotNull(jsonApiOptions);

        _jsonApiRoutingConvention = jsonApiRoutingConvention;
        _openApiEndpointConvention = openApiEndpointConvention;
        _jsonApiRequestFormatMetadataProvider = jsonApiRequestFormatMetadataProvider;
        _jsonApiOptions = jsonApiOptions;
    }

    public void Configure(MvcOptions options)
    {
        AddSwashbuckleCliCompatibility(options);

        options.InputFormatters.Add(_jsonApiRequestFormatMetadataProvider);
        options.Conventions.Add(_openApiEndpointConvention);

        // TODO: Only when resource graph has inheritance?
        ((JsonApiOptions)_jsonApiOptions).IncludeExtensions(JsonApiExtension.OpenApi, JsonApiExtension.RelaxedOpenApi);
    }

    private void AddSwashbuckleCliCompatibility(MvcOptions options)
    {
        if (!options.Conventions.Any(convention => convention is IJsonApiRoutingConvention))
        {
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1957 for why this is needed.
            options.Conventions.Insert(0, _jsonApiRoutingConvention);
        }
    }
}
