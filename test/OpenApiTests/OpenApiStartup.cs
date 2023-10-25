using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestBuildingBlocks;

namespace OpenApiTests;

public class OpenApiStartup<TDbContext> : TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    public override void ConfigureServices(IServiceCollection services)
    {
        IMvcCoreBuilder mvcBuilder = services.AddMvcCore();

        services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcBuilder);

        services.AddOpenApi(mvcBuilder, SetupSwaggerGenAction);
    }

    protected virtual void SetupSwaggerGenAction(SwaggerGenOptions options)
    {
    }

    public override void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseSwagger();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
