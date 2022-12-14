using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks;

public class TestableStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        IMvcCoreBuilder mvcBuilder = services.AddMvcCore(options => options.MaxModelValidationErrors = 3);

        services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcBuilder);
    }

    protected virtual void SetJsonApiOptions(JsonApiOptions options)
    {
        options.IncludeExceptionStackTraceInErrors = true;
        options.IncludeRequestBodyInErrors = true;
        options.SerializerOptions.WriteIndented = true;
    }

    public virtual void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
