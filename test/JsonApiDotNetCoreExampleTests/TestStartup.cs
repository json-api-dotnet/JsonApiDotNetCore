using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnitTests;

namespace JsonApiDotNetCoreExampleTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env) : base(env)
        { }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddClientSerialization();
            services.AddScoped<IScopedServiceProvider, TestScopedServiceProvider>();
            return services.BuildServiceProvider();
        }
    }
}
