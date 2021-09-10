using JsonApiDotNetCore.OpenApiClient;
using Newtonsoft.Json;

namespace OpenApiTests.ClientLibrary.GeneratedCode
{
    // ReSharper disable once MemberCanBeInternal
    public partial class OpenApiClient : JsonApiClient
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
