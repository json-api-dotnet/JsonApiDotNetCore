using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreExample
{
    public class Startup : EmptyStartup
    {
        private readonly string _connectionString;

        public Startup(IConfiguration configuration) : base(configuration)
        {
            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            _connectionString = configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            ConfigureClock(services);

            services.AddScoped<SkipCacheQueryStringParameterReader>();
            services.AddScoped<IQueryStringParameterReader>(sp => sp.GetRequiredService<SkipCacheQueryStringParameterReader>());

            services.AddDbContext<AppDbContext>(options =>
            {
                options.EnableSensitiveDataLogging();
                options.UseNpgsql(_connectionString, innerOptions => innerOptions.SetPostgresVersion(new Version(9, 6)));
            },
                // TODO: Remove ServiceLifetime.Transient, after all integration tests have been converted to use IntegrationTestContext.
                ServiceLifetime.Transient);

            services.AddJsonApi<AppDbContext>(ConfigureJsonApiOptions, discovery => discovery.AddCurrentAssembly());

            // once all tests have been moved to WebApplicationFactory format we can get rid of this line below
            services.AddClientSerialization();
        }

        protected virtual void ConfigureClock(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, SystemClock>();
        }

        protected virtual void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(5);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                appDbContext.Database.EnsureCreated();
            }
            
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
