using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System;
using OperationsExample.Data;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Formatters;

namespace OperationsExample
{
    public class Startup
    {
        public readonly IConfiguration Config;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
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
            }, ServiceLifetime  .Transient);

            services.AddJsonApi<AppDbContext>(opt => opt.EnableExtension(JsonApiExtension.Operations));

            services.AddMvc().AddMvcOptions(options => {
                    options.InputFormatters.Clear();
                    options.InputFormatters.Insert(0, new JsonApiOperationsInputFormatter());
                });

            return services.BuildServiceProvider();
        }

        public virtual void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            AppDbContext context)
        {
            context.Database.EnsureCreated();

            loggerFactory.AddConsole(Config.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseJsonApi();
        }

        public string GetDbConnectionString() => Config["Data:DefaultConnection"];
    }
}