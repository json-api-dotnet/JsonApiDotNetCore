using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesEnabled.ModelStateValidationDisabled.GeneratedCode;

internal partial class NrtOnMsvOffClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
