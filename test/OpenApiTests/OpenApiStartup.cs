using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using TestBuildingBlocks;

namespace OpenApiTests
{
    public sealed class OpenApiStartup<TDbContext> : TestableStartup<TDbContext>
        where TDbContext : DbContext
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            IMvcCoreBuilder mvcBuilder = services.AddMvcCore();

            services.AddJsonApi<TDbContext>(SetJsonApiOptions, mvcBuilder: mvcBuilder);

            services.AddOpenApi(mvcBuilder);
        }

        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(10);
            options.MaximumPageSize = new PageSize(100);
            options.MaximumPageNumber = new PageNumber(50);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.DefaultAttrCapabilities = AttrCapabilities.AllowView;

            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            };
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
