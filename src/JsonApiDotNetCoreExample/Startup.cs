using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Routing;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Reflection;
using System;

namespace JsonApiDotNetCoreExample
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _config = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => {
                options.UseNpgsql(_getDbConnectionString());
            }, ServiceLifetime.Transient);
            
            services.AddJsonApi<AppDbContext>(opt => {
                opt.Namespace = "api/v1";
                opt.DefaultPageSize = 1;
            });
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            AppDbContext context)
        {
            context.Database.Migrate();

            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseJsonApi();
        }

        private string _getDbConnectionString()
        {
            return _config["Data:DefaultConnection"];
        }
    }
}