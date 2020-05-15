using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GettingStarted
{
    public sealed class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SampleDbContext>(
                options => options.UseSqlite("Data Source=sample.db"));

            services.AddJsonApi<SampleDbContext>(
                options => options.Namespace = "api");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, SampleDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            CreateSampleData(context);

            app.UseJsonApi();
        }

        private static void CreateSampleData(SampleDbContext context)
        {
            context.Articles.AddRange(new Article
            {
                Title = "What's new in JsonApiDotNetCore",
                Author = new Person
                {
                    Name = "John Doe"
                }
            }, new Article
            {
                Title = ".NET Core Best Practices",
                Author = new Person
                {
                    Name = "Microsoft"
                }
            });

            context.SaveChanges();
        }
    }
}
