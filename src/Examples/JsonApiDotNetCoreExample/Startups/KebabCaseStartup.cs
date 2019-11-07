using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System;
using Microsoft.Extensions.Logging.Debug;
using JsonApiDotNetCore.Graph;

namespace JsonApiDotNetCoreExample
{
    public class KebabCaseStartup
    {
        public readonly IConfiguration Config;

        public KebabCaseStartup(IConfiguration configuration)
        {
            Config = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            services
                .AddSingleton<IResourceNameFormatter, KebabCaseFormatter>()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddConfiguration(Config.GetSection("Logging"));
                })
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseLoggerFactory(new LoggerFactory(new[] { new DebugLoggerProvider() }))
                           .EnableSensitiveDataLogging()
                           .UseNpgsql(GetDbConnectionString(), options => options.SetPostgresVersion(new Version(9, 6)));
                }, ServiceLifetime.Transient)
                .AddJsonApi(options =>
                {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
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
