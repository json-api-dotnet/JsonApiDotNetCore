using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleClient;

[UsedImplicitly(ImplicitUseTargetFlags.Itself)]
public partial class ExampleApiClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
