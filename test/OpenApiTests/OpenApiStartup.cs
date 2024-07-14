using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle;
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
        base.ConfigureServices(services);

        services.AddOpenApiForJsonApi(SetupSwaggerGenAction);
    }

    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.UseRelativeLinks = true;
        options.IncludeTotalResourceCount = true;
    }

    protected virtual void SetupSwaggerGenAction(SwaggerGenOptions options)
    {
        string documentationPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".xml");
        options.IncludeXmlComments(documentationPath);
    }

    public override void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseSwagger();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
