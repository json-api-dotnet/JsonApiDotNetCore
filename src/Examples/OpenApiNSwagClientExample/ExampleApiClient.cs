using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Newtonsoft.Json;

namespace OpenApiNSwagClientExample;

[UsedImplicitly(ImplicitUseTargetFlags.Itself)]
public partial class ExampleApiClient : JsonApiClient
{
    partial void Initialize()
    {
        _instanceSettings = new JsonSerializerSettings(_settings.Value);

#if DEBUG
        _instanceSettings.Formatting = Formatting.Indented;
#endif

        SetSerializerSettingsForJsonApi(_instanceSettings);
    }
}
