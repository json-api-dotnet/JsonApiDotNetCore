using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.Services;

namespace JsonApiDotNetCoreExampleTests.Startups
{
    public class MetaStartup : Startup
    {
        public MetaStartup(IWebHostEnvironment env)
        : base (env)
        {  }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRequestMeta, MetaService>();
            base.ConfigureServices(services);
        }
    }
}
