using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

public sealed class IdCompactionStartup : TestableStartup<IdCompactionDbContext>
{
    protected override void AddJsonApi(IServiceCollection services)
    {
        services.AddJsonApi<IdCompactionDbContext>(ConfigureJsonApiOptions, resources: builder => builder.Remove<CompactIdentifiable>());
    }
}
