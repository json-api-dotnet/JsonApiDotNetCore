using System;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using NoEntityFrameworkExample.Services;

namespace NoEntityFrameworkExample
{
    public class Startup
    {
        private readonly string _connectionString;

        public Startup(IConfiguration configuration)
        {
            _connectionString = configuration["Data:DefaultConnection"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonApi(
                options => options.Namespace = "api/v1",
                resources: builder => builder.AddResource<TodoItem>("todoItems")
            );

            services.AddScoped<IResourceService<TodoItem>, TodoItemService>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString,
                    postgresOptions => postgresOptions.SetPostgresVersion(new Version(9, 6)));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, AppDbContext context)
        {
            context.Database.EnsureCreated();

            app.UseJsonApi();
        }
    }
}
