using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NoEntityFrameworkExample.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace NoEntityFrameworkExample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            var mvcBuilder = services.AddMvcCore();

            services.AddJsonApi(options => {
                options.Namespace = "api/v1";
                options.BuildResourceGraph((builder) => {
                    builder.AddResource<TodoItem>("custom-todo-items");
                });
            }, mvcBuilder);

            services.AddScoped<IResourceService<TodoItem>, TodoItemService>();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>(); 
            optionsBuilder.UseNpgsql(Configuration.GetValue<string>("Data:DefaultConnection")); 
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<DbContextOptions<AppDbContext>>(optionsBuilder.Options);
            services.AddScoped<AppDbContext>();

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppDbContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            context.Database.EnsureCreated();

            app.UseMvc();
        }
    }
}
