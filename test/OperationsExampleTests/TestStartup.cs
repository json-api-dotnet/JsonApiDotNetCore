using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OperationsExample;
using OperationsExample.Data;
using System;
using UnitTests;

namespace OperationsExampleTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IHostingEnvironment env) : base(env)
        { }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScoped<IScopedServiceProvider, TestScopedServiceProvider>();
            services.AddSingleton<IDbContextResolver, DbContextResolver<AppDbContext>>();
            return services.BuildServiceProvider();
        }
    }
}
