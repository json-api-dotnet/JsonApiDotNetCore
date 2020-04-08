using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
    public sealed class NoDefaultPageSizeStartup : Startup
    {
        public NoDefaultPageSizeStartup(IWebHostEnvironment env) : base(env) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddMvcCore();
            services
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(GetDbConnectionString()), ServiceLifetime.Transient)
                .AddJsonApi(options => {
                    options.Namespace = "api/v1";
                    options.IncludeTotalRecordCount = true;
                    options.LoadDatabaseValues = true;
                    options.AllowClientGeneratedIds = true;
                    options.DefaultPageSize = 0;
                },
                discovery => discovery.AddAssembly(Assembly.Load(nameof(JsonApiDotNetCoreExample))),
                mvcBuilder: mvcBuilder);
        }
    }
}
