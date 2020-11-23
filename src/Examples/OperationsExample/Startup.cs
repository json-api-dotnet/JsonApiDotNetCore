using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            loggerFactory.AddConsole(LogLevel.Warning);

            services.AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(GetDbConnectionString()), ServiceLifetime.Scoped);

            services.AddJsonApi<AppDbContext>(opt => opt.EnableOperations = true);

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
            app.UseJsonApi();
        }

        public string GetDbConnectionString() => Config["Data:DefaultConnection"];
    }
}
