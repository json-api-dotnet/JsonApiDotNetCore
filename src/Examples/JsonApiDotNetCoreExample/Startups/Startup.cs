using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var loggerFactory = new LoggerFactory();
            services
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddConfiguration(Config.GetSection("Logging"));
                })
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(GetDbConnectionString(), options => options.SetPostgresVersion(new Version(9,6)));
                }, ServiceLifetime.Transient)
                .AddJsonApi(options =>
                {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
                    options.EnableResourceHooks = true;
                    options.LoadDatabaseValues = true;
                },
                discovery => discovery.AddCurrentAssembly());
            services.AddClientSerialization();
        }

        public virtual void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            AppDbContext context)
        {

            context.Database.EnsureCreated();
            app.UseJsonApi();
        }

        public string GetDbConnectionString() => Config["Data:DefaultConnection"];
    }
}
