using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using UnitTests;

namespace JsonApiDotNetCoreExampleTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IWebHostEnvironment env) : base(env)
        { }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddClientSerialization();
            services.AddScoped<IScopedServiceProvider, TestScopedServiceProvider>();
        }
    }
}
