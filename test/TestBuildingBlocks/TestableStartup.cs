using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks
{
    public class TestableStartup<TDbContext>
        where TDbContext : DbContext
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

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment environment, ILoggerFactory loggerFactory)
        {
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
