using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class MixedControllerStartup : OpenApiStartup<CoffeeDbContext>
{
    protected override void AddJsonApi(IServiceCollection services)
    {
        services.AddJsonApi<CoffeeDbContext>(ConfigureJsonApiOptions, resources: builder => builder.Add<CoffeeSummary, long>());
    }
}
