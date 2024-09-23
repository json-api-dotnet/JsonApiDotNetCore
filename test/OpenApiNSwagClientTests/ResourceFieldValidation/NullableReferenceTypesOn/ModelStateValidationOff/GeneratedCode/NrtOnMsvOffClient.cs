using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;

#pragma warning disable CA1852 // Seal internal types

namespace OpenApiNSwagClientTests.ResourceFieldValidation.NullableReferenceTypesOn.ModelStateValidationOff.GeneratedCode;

internal partial class NrtOnMsvOffClient : JsonApiClient
{
    partial void Initialize()
    {
        _instanceSettings = new JsonSerializerSettings(_settings.Value)
        {
            Formatting = Formatting.Indented
        };

        SetSerializerSettingsForJsonApi(_instanceSettings);
    }
}
