using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class KebabCasingConventionStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.Namespace = "public-api";
        options.UseRelativeLinks = true;
        options.IncludeTotalResourceCount = true;

        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
        options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.KebabCaseLower;
    }
}
