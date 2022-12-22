using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

namespace OpenApiClientTests.SchemaProperties.NullableReferenceTypesEnabled.RelationshipsObject.GeneratedCode;

internal partial class NullableReferenceTypesEnabledClientRelationshipsObject : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }
}
