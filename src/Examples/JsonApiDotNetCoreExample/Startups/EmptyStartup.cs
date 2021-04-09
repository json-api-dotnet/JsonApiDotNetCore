using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExample.Startups
{
    /// <summary>
    /// Empty startup class, required for integration tests. Changes in ASP.NET Core 3 no longer allow Startup class to be defined in test projects. See
    /// https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public abstract class EmptyStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
        }
    }
}
