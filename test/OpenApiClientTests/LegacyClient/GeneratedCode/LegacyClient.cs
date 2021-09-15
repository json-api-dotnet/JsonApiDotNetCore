using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace OpenApiClientTests.LegacyClient.GeneratedCode
{
    internal partial class LegacyClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);

#if DEBUG
            settings.Formatting = Formatting.Indented;
#endif
        }
    }
}
