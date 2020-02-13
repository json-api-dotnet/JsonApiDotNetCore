using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System;

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
            services
                .AddDbContext<AppDbContext>(options =>
                {
                    options
                        .EnableSensitiveDataLogging()
                        .UseNpgsql(GetDbConnectionString(), options => options.SetPostgresVersion(new Version(9,6)));
                }, ServiceLifetime.Transient)
                .AddJsonApi(options =>
                {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
                    options.LoadDatabaseValues = true;
                },
                discovery => discovery.AddCurrentAssembly());
            // once all tests have been moved to WebApplicationFactory format we can get rid of this line below
            services.AddClientSerialization(); 
        }

        public virtual void Configure(
            IApplicationBuilder app,
            AppDbContext context)
        {
            context.Database.EnsureCreated();
            app.EnableDetailedErrors();
            app.UseJsonApi();
        }

        public string GetDbConnectionString() => Config["Data:DefaultConnection"];
    }
}
