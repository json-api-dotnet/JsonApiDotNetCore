using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleClient.GeneratedCode
{
    // ReSharper disable once MemberCanBeInternal
    public partial class ExampleApiClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);

            settings.Formatting = Formatting.Indented;
        }
    }
}
