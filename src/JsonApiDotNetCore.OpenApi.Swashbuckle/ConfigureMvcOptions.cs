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

    public ConfigureMvcOptions(IJsonApiRoutingConvention jsonApiRoutingConvention, OpenApiEndpointConvention openApiEndpointConvention,
        JsonApiRequestFormatMetadataProvider jsonApiRequestFormatMetadataProvider)
    {
        ArgumentNullException.ThrowIfNull(jsonApiRoutingConvention);
        ArgumentNullException.ThrowIfNull(openApiEndpointConvention);
        ArgumentNullException.ThrowIfNull(jsonApiRequestFormatMetadataProvider);

        _jsonApiRoutingConvention = jsonApiRoutingConvention;
        _openApiEndpointConvention = openApiEndpointConvention;
        _jsonApiRequestFormatMetadataProvider = jsonApiRequestFormatMetadataProvider;
    }

    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        AddSwashbuckleCliCompatibility(options);

        options.InputFormatters.Add(_jsonApiRequestFormatMetadataProvider);
        options.Conventions.Add(_openApiEndpointConvention);
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
