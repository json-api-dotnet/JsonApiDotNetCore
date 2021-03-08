using GettingStarted.Data;
using GettingStarted.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GettingStarted
{
    public sealed class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SampleDbContext>(options => options.UseSqlite("Data Source=sample.db"));

            services.AddJsonApi<SampleDbContext>(options =>
            {
                options.Namespace = "api";
                options.UseRelativeLinks = true;
                options.IncludeTotalResourceCount = true;
                options.SerializerSettings.Formatting = Formatting.Indented;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, SampleDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            CreateSampleData(context);

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static void CreateSampleData(SampleDbContext context)
        {
            // Note: The generate-examples.ps1 script (to create example requests in documentation) depends on these.

            context.Books.AddRange(new Book
            {
                Title = "Frankenstein",
                PublishYear = 1818,
                Author = new Person
                {
                    Name = "Mary Shelley"
                }
            }, new Book
            {
                Title = "Robinson Crusoe",
                PublishYear = 1719,
                Author = new Person
                {
                    Name = "Daniel Defoe"
                }
            }, new Book
            {
                Title = "Gulliver's Travels",
                PublishYear = 1726,
                Author = new Person
                {
                    Name = "Jonathan Swift"
                }
            });

            context.SaveChanges();
        }
    }
}
