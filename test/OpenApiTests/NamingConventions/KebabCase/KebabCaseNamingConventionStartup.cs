using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using OpenApiTests.LegacyOpenApiIntegration;
using TestBuildingBlocks;

namespace OpenApiTests.NamingConventions.KebabCase;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class KebabCaseNamingConventionStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.SerializerOptions.PropertyNamingPolicy = JsonKebabCaseNamingPolicy.Instance;
        options.SerializerOptions.DictionaryKeyPolicy = JsonKebabCaseNamingPolicy.Instance;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
