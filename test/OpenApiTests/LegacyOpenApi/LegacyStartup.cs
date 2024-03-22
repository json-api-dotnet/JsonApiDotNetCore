using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using OpenApiTests.NamingConventions.KebabCase;
using TestBuildingBlocks;

namespace OpenApiTests.LegacyOpenApi;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class LegacyStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.Namespace = "api";
        options.DefaultAttrCapabilities = AttrCapabilities.AllowView;
        options.IncludeJsonApiVersion = true;
        options.SerializerOptions.PropertyNamingPolicy = JsonKebabCaseNamingPolicy.Instance;
        options.SerializerOptions.DictionaryKeyPolicy = JsonKebabCaseNamingPolicy.Instance;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
