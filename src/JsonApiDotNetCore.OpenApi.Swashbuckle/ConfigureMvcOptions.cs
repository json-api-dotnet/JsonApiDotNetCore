using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly JsonApiRequestFormatMetadataProvider _jsonApiRequestFormatMetadataProvider;
    private readonly JsonApiOptions _jsonApiOptions;

    public ConfigureMvcOptions(JsonApiRequestFormatMetadataProvider jsonApiRequestFormatMetadataProvider, IJsonApiOptions jsonApiOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonApiRequestFormatMetadataProvider);
        ArgumentNullException.ThrowIfNull(jsonApiOptions);

        _jsonApiRequestFormatMetadataProvider = jsonApiRequestFormatMetadataProvider;
        _jsonApiOptions = (JsonApiOptions)jsonApiOptions;
    }

    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.InputFormatters.Add(_jsonApiRequestFormatMetadataProvider);

        _jsonApiOptions.IncludeExtensions(OpenApiMediaTypeExtension.OpenApi, OpenApiMediaTypeExtension.RelaxedOpenApi);
    }
}
