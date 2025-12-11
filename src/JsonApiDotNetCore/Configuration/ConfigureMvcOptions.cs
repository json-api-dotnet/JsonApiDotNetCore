using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration;

internal sealed class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly IJsonApiInputFormatter _inputFormatter;
    private readonly IJsonApiOutputFormatter _outputFormatter;
    private readonly IJsonApiRoutingConvention _routingConvention;

    public ConfigureMvcOptions(IJsonApiInputFormatter inputFormatter, IJsonApiOutputFormatter outputFormatter, IJsonApiRoutingConvention routingConvention)
    {
        ArgumentNullException.ThrowIfNull(inputFormatter);
        ArgumentNullException.ThrowIfNull(outputFormatter);
        ArgumentNullException.ThrowIfNull(routingConvention);

        _inputFormatter = inputFormatter;
        _outputFormatter = outputFormatter;
        _routingConvention = routingConvention;
    }

    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnableEndpointRouting = true;

        options.InputFormatters.Insert(0, _inputFormatter);
        options.OutputFormatters.Insert(0, _outputFormatter);
        options.Conventions.Insert(0, _routingConvention);

        options.Filters.AddService<IAsyncJsonApiExceptionFilter>();
        options.Filters.AddService<IAsyncQueryStringActionFilter>();
        options.Filters.AddService<IAsyncConvertEmptyActionResultFilter>();
    }
}
