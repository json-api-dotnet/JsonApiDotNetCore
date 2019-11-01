using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Extensions;
using System.Reflection;

namespace JsonApiDotNetCoreExample
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public class ClientGeneratedIdsStartup : Startup
    {
        public ClientGeneratedIdsStartup(IWebHostEnvironment env)
        : base(env)
        { }

        public override void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            var mvcBuilder = services.AddMvcCore();
            services
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                })
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(GetDbConnectionString()), ServiceLifetime.Transient)
                .AddJsonApi(options => {
                    options.Namespace = "api/v1";
                    options.DefaultPageSize = 5;
                    options.IncludeTotalRecordCount = true;
                    options.EnableResourceHooks = true;
                    options.LoadDatabase = true;
                    options.AllowClientGeneratedIds = true;
                },
                discovery => discovery.AddAssembly(Assembly.Load(nameof(JsonApiDotNetCoreExample))),
                mvcBuilder: mvcBuilder);
        }
    }
}