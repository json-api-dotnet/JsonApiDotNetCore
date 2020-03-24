using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NoEntityFrameworkExample;

namespace NoEntityFrameworkTests
{
    public sealed class TestStartup : Startup
    {
        public TestStartup(IWebHostEnvironment env) : base(env)
        { }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScoped<IScopedServiceProvider, TestScopedServiceProvider>();
            services.BuildServiceProvider();
        }
    }
}
