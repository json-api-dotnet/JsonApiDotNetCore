using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;

namespace OpenApiClientTests.ResourceFieldValidation.NullableReferenceTypesOff.ModelStateValidationOn.GeneratedCode;

internal partial class NrtOffMsvOnClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
