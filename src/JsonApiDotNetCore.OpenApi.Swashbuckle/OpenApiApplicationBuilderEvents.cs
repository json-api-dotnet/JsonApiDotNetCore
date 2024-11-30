using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class OpenApiApplicationBuilderEvents : IJsonApiApplicationBuilderEvents
{
    private readonly IJsonApiOptions _options;
    private readonly IJsonApiRequestAccessor _requestAccessor;

    public OpenApiApplicationBuilderEvents(IJsonApiOptions options, IJsonApiRequestAccessor requestAccessor)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(requestAccessor);

        _options = options;
        _requestAccessor = requestAccessor;
    }

    public void ResourceGraphBuilt(IResourceGraph resourceGraph)
    {
        _options.SerializerOptions.Converters.Add(new OpenApiResourceObjectConverter(resourceGraph, _requestAccessor));
    }
}
