using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class HostingStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.Namespace = "public-api";
        options.IncludeTotalResourceCount = true;
    }

    public override void Configure(IApplicationBuilder app)
    {
        app.UsePathBase("/iis-application-virtual-directory");

        base.Configure(app);
    }
}
