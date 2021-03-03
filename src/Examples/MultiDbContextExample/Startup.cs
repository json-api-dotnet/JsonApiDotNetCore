using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiDbContextExample.Data;
using MultiDbContextExample.Models;
using MultiDbContextExample.Repositories;

namespace MultiDbContextExample
{
    public sealed class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DbContextA>(options => options.UseSqlite("Data Source=A.db"));
            services.AddDbContext<DbContextB>(options => options.UseSqlite("Data Source=B.db"));

            services.AddResourceRepository<DbContextARepository<ResourceA>>();
            services.AddResourceRepository<DbContextBRepository<ResourceB>>();

            services.AddJsonApi(options =>
            {
                options.IncludeExceptionStackTraceInErrors = true;
            }, dbContextTypes: new[]
            {
                typeof(DbContextA),
                typeof(DbContextB)
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbContextA dbContextA, DbContextB dbContextB)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            EnsureSampleDataA(dbContextA);
            EnsureSampleDataB(dbContextB);

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static void EnsureSampleDataA(DbContextA dbContextA)
        {
            dbContextA.Database.EnsureDeleted();
            dbContextA.Database.EnsureCreated();

            dbContextA.ResourceAs.Add(new ResourceA
            {
                NameA = "SampleA"
            });

            dbContextA.SaveChanges();
        }

        private static void EnsureSampleDataB(DbContextB dbContextB)
        {
            dbContextB.Database.EnsureDeleted();
            dbContextB.Database.EnsureCreated();

            dbContextB.ResourceBs.Add(new ResourceB
            {
                NameB = "SampleB"
            });

            dbContextB.SaveChanges();
        }
    }
}
