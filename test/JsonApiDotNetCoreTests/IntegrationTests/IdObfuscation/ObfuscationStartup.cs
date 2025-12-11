using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

public sealed class ObfuscationStartup : TestableStartup<ObfuscationDbContext>
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAtomicOperationFilter, ObfuscationOperationFilter>();
        base.ConfigureServices(services);
    }

    protected override void AddJsonApi(IServiceCollection services)
    {
        services.AddJsonApi<ObfuscationDbContext>(ConfigureJsonApiOptions, resources: builder => builder.Remove<ObfuscatedIdentifiable>());
    }
}
