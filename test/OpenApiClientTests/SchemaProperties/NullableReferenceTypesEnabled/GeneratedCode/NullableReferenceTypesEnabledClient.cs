using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.GeneratedCode;

internal partial class NullableReferenceTypesEnabledClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
