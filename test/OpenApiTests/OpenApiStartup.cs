using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TestBuildingBlocks;

namespace OpenApiTests
{
    public sealed class OpenApiStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        internal const string OpenApiDocumentName = nameof(OpenApiTests);

        public override void ConfigureServices(IServiceCollection services)
        {
            IMvcCoreBuilder mvcCoreBuilder = services.AddMvcCore();

            services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcCoreBuilder);

            services.AddOpenApi(mvcCoreBuilder, options => options.SwaggerDoc(OpenApiDocumentName, new OpenApiInfo
            {
                Title = OpenApiDocumentName,
                Version = "1"
            }));
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment, ILoggerFactory loggerFactory)
        {
            app.UseRouting();
            app.UseJsonApi();
            app.UseSwagger();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
