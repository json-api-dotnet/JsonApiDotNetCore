using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ScopesStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    public override void ConfigureServices(IServiceCollection services)
    {
        IMvcCoreBuilder mvcBuilder = services.AddMvcCore(options => options.Filters.Add<ScopesAuthorizationFilter>(int.MaxValue));

        services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcBuilder);
    }
}
