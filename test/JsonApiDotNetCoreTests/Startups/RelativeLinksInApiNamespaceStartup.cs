using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.Startups;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class RelativeLinksInApiNamespaceStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.Namespace = "api";
        options.UseRelativeLinks = true;
    }
}
