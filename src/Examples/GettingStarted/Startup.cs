using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore;

namespace GettingStarted
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SampleDbContext>(options =>
            {
                options.UseSqlite("Data Source=sample.db");
            });

            var mvcBuilder = services.AddMvcCore();
            services.AddJsonApi(
                options => options.Namespace = "api", 
                discover => discover.AddCurrentAssembly(), mvcBuilder: mvcBuilder);
        }

        public void Configure(IApplicationBuilder app, SampleDbContext context)
        {
            context.Database.EnsureDeleted(); // indices need to be reset
            app.UseJsonApi();
        }
    }
}
