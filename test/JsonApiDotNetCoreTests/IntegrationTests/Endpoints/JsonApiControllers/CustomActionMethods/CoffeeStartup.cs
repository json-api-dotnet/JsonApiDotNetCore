using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CoffeeStartup : TestableStartup<CoffeeDbContext>
{
    protected override void AddJsonApi(IServiceCollection services)
    {
        services.AddJsonApi<CoffeeDbContext>(ConfigureJsonApiOptions, resources: builder => builder.Add<CoffeeSummary, long>());
    }

    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);
        options.AllowUnknownQueryStringParameters = true;
    }
}
