using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using NoEntityFrameworkExample.Services;

namespace NoEntityFrameworkExample
{
    public sealed class Startup
    {
        private readonly string _connectionString;

        public Startup(IConfiguration configuration)
        {
            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            _connectionString = configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonApi(options => options.Namespace = "api/v1", resources: builder => builder.Add<WorkItem>("workItems"));

            services.AddResourceService<WorkItemService>();

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, AppDbContext dbContext)
        {
            dbContext.Database.EnsureCreated();

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
