using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenApiTests.IdObfuscation;

public sealed class ObfuscationStartup : OpenApiStartup<ObfuscationDbContext>
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
