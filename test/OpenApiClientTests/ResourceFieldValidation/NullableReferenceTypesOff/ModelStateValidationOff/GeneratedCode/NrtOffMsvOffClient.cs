using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOff.GeneratedCode;

internal partial class NrtOffMsvOffClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
