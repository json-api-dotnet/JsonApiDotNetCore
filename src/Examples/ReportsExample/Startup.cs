using JsonApiDotNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ReportsExample
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonApi(
                options => options.Namespace = "api",
                discovery => discovery.AddCurrentAssembly());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseJsonApi();
        }
    }
}
