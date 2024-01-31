using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiRoutingConvention _jsonApiRoutingConvention;

    public ConfigureMvcOptions(IControllerResourceMapping controllerResourceMapping, IJsonApiRoutingConvention jsonApiRoutingConvention)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(jsonApiRoutingConvention);

        _controllerResourceMapping = controllerResourceMapping;
        _jsonApiRoutingConvention = jsonApiRoutingConvention;
    }

    public void Configure(MvcOptions options)
    {
        AddSwashbuckleCliCompatibility(options);
        AddOpenApiEndpointConvention(options);
    }

    private void AddSwashbuckleCliCompatibility(MvcOptions options)
    {
        if (!options.Conventions.Any(convention => convention is IJsonApiRoutingConvention))
        {
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1957 for why this is needed.
            options.Conventions.Insert(0, _jsonApiRoutingConvention);
        }
    }

    private void AddOpenApiEndpointConvention(MvcOptions options)
    {
        var convention = new OpenApiEndpointConvention(_controllerResourceMapping);
        options.Conventions.Add(convention);
    }
}
