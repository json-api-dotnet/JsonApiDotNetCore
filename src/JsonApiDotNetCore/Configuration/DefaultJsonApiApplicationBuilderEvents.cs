using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Configuration;

internal sealed class DefaultJsonApiApplicationBuilderEvents : IJsonApiApplicationBuilderEvents
{
    private readonly IJsonApiOptions _options;

    public DefaultJsonApiApplicationBuilderEvents(IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(options);

        _options = options;
    }

    public void ResourceGraphBuilt(IResourceGraph resourceGraph)
    {
        _options.SerializerOptions.Converters.Add(new ResourceObjectConverter(resourceGraph));
    }
}
