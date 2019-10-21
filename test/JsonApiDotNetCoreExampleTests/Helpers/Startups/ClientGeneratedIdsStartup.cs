using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System;
using JsonApiDotNetCoreExample;
using System.Reflection;

namespace JsonApiDotNetCoreExampleTests.Startups
{
    public class ClientGeneratedIdsStartup : Startup
    {
        public ClientGeneratedIdsStartup(IHostingEnvironment env)
        : base (env)
        {  }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Warning);
            var mvcBuilder = services.AddMvcCore();
            services
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(GetDbConnectionString()), ServiceLifetime.Transient)
                .AddJsonApi(options => {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
                    options.EnableResourceHooks = true;
                    options.LoaDatabaseValues = true;
                    options.AllowClientGeneratedIds = true;
                },
                discovery => discovery.AddAssembly(Assembly.Load(nameof(JsonApiDotNetCoreExample))),
                mvcBuilder: mvcBuilder);

            return services.BuildServiceProvider();

        }
    }
}
