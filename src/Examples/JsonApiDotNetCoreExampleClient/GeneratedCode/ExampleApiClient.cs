using JsonApiDotNetCore.OpenApiClient;
using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleClient.GeneratedCode
{
    public partial class ExampleApiClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);

            settings.Formatting = Formatting.Indented;
        }
    }
}
