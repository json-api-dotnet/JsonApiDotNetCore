using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesDisabled.RelationshipsObject.GeneratedCode;

internal partial class NullableReferenceTypesDisabledClientRelationshipsObject : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
