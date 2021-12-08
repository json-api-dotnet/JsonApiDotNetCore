using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;

namespace OpenApiTests;

public abstract class OpenApiStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : DbContext
{
    public override void ConfigureServices(IServiceCollection services)
    {
        IMvcCoreBuilder mvcBuilder = services.AddMvcCore();

        services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcBuilder);

        services.AddOpenApi(mvcBuilder);
    }

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseSwagger();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
