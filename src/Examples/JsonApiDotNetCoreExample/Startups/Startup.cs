using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreExample.Startups
{
    public sealed class Startup : EmptyStartup
    {
        private readonly string _connectionString;

        public Startup(IConfiguration configuration)
        {
            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            _connectionString = configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, SystemClock>();

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_connectionString));

            services.AddJsonApi<AppDbContext>(ConfigureJsonApiOptions, discovery => discovery.AddCurrentAssembly());
        }

        private void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(5);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                appDbContext.Database.EnsureCreated();
            }

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
