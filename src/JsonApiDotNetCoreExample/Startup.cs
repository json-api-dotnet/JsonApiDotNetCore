using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Routing;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using DotNetCoreDocs.Configuration;
using DotNetCoreDocs.Middleware;
using System;

namespace JsonApiDotNetCoreExample
{
    public class Startup
    {
        public readonly IConfiguration Config;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Config = builder.Build();
        }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory
              .AddConsole(LogLevel.Trace);
            services.AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(GetDbConnectionString());
            }, ServiceLifetime.Transient);

            services.AddJsonApi<AppDbContext>(opt =>
            {
                opt.Namespace = "api/v1";
                opt.DefaultPageSize = 5;
                opt.IncludeTotalRecordCount = true;
            });

            services.AddDocumentationConfiguration(Config);

            var provider = services.BuildServiceProvider();
            var appContext = provider.GetRequiredService<AppDbContext>();
            if(appContext == null)
                throw new ArgumentException();
                
            return provider;
        }

        public virtual void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            AppDbContext context)
        {
            context.Database.Migrate();

            loggerFactory.AddConsole(Config.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDocs();

            app.UseJsonApi();
        }

        public string GetDbConnectionString()
        {
            return Config["Data:DefaultConnection"];
        }
    }
}