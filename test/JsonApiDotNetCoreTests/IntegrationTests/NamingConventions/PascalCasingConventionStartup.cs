using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PascalCasingConventionStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : DbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.Namespace = "PublicApi";
        options.UseRelativeLinks = true;
        options.IncludeTotalResourceCount = true;

        options.SerializerOptions.PropertyNamingPolicy = null;
        options.SerializerOptions.DictionaryKeyPolicy = null;
    }
}
