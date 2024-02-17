using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration;

namespace JsonApiDotNetCoreExampleClient;

[UsedImplicitly(ImplicitUseTargetFlags.Itself)]
public partial class ExampleApiClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

#if DEBUG
        settings.Formatting = Formatting.Indented;
#endif
    }
}

public class TestParameterNameGenerator : IParameterNameGenerator
{
    public string Generate(OpenApiParameter parameter, IEnumerable<OpenApiParameter> allParameters)
    {
        throw new NotImplementedException("TTEESSST");
    }
}
