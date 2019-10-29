using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnitTests;

namespace JsonApiDotNetCoreExampleTests
{
    public class CamelCaseTestStartup : Startup
    {
        public CamelCaseTestStartup(IWebHostEnvironment env) : base(env)
        { }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IResourceNameFormatter, CamelCaseFormatter>();
            base.ConfigureServices(services);
            services.AddClientSerialization();
            services.AddScoped<IScopedServiceProvider, TestScopedServiceProvider>();
        }
    }
}
