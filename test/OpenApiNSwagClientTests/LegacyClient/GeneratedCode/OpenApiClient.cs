using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;

namespace OpenApiNSwagClientTests.LegacyClient.GeneratedCode;

internal partial class OpenApiClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
