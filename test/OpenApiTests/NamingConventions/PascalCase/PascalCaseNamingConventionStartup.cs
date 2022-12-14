using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace OpenApiTests.NamingConventions.PascalCase;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PascalCaseNamingConventionStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.SerializerOptions.PropertyNamingPolicy = null;
        options.SerializerOptions.DictionaryKeyPolicy = null;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
