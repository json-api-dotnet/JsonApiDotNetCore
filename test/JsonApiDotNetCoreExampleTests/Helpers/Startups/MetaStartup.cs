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
        public MetaStartup(IHostingEnvironment env)
        : base (env)
        {  }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRequestMeta, MetaService>();
            return base.ConfigureServices(services);
        }
    }
}
