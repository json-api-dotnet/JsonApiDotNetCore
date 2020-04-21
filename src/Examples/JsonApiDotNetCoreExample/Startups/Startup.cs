using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCoreExample.Services;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreExample
{
    public class Startup
    {
        public readonly IConfiguration Config;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Config = builder.Build();
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<SkipCacheQueryParameterService>();
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<SkipCacheQueryParameterService>());

            services
                .AddDbContext<AppDbContext>(options =>
                {
                    options
                        .EnableSensitiveDataLogging()
                        .UseNpgsql(GetDbConnectionString(), innerOptions => innerOptions.SetPostgresVersion(new Version(9,6)));
                }, ServiceLifetime.Transient)
                .AddJsonApi<AppDbContext>(ConfigureJsonApiOptions, discovery => discovery.AddCurrentAssembly());
            
            // once all tests have been moved to WebApplicationFactory format we can get rid of this line below
            services.AddClientSerialization();
        }

        protected virtual void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "api/v1";
            options.DefaultPageSize = 5;
            options.IncludeTotalRecordCount = true;
            options.LoadDatabaseValues = true;
            options.ValidateModelState = true;
            options.EnableResourceHooks = true;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public void Configure(
            IApplicationBuilder app,
            AppDbContext context)
        {
            context.Database.EnsureCreated();
            app.UseJsonApi();
        }

        public string GetDbConnectionString() => Config["Data:DefaultConnection"];
    }
}
