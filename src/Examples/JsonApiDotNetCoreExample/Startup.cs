using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreExample
{
    public sealed class Startup
    {
        private readonly ICodeTimerSession _codeTimingSession;
        private readonly string _connectionString;

        public Startup(IConfiguration configuration)
        {
            _codeTimingSession = new DefaultCodeTimerSession();
            CodeTimingSessionManager.Capture(_codeTimingSession);

            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            _connectionString = configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (CodeTimingSessionManager.Current.Measure("Configure other (startup)"))
            {
                services.AddSingleton<ISystemClock, SystemClock>();

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
#if DEBUG
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
#endif
                });

                using (CodeTimingSessionManager.Current.Measure("Configure JSON:API (startup)"))
                {
                    services.AddJsonApi<AppDbContext>(options =>
                    {
                        options.Namespace = "api/v1";
                        options.UseRelativeLinks = true;
                        options.ValidateModelState = true;
                        options.IncludeTotalResourceCount = true;
                        options.SerializerSettings.Formatting = Formatting.Indented;
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
#if DEBUG
                        options.IncludeExceptionStackTraceInErrors = true;
#endif
                    }, discovery => discovery.AddCurrentAssembly());
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment, ILoggerFactory loggerFactory)
        {
            ILogger<Startup> logger = loggerFactory.CreateLogger<Startup>();

            using (CodeTimingSessionManager.Current.Measure("Initialize other (startup)"))
            {
                using (IServiceScope scope = app.ApplicationServices.CreateScope())
                {
                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    appDbContext.Database.EnsureCreated();
                }

                app.UseRouting();

                using (CodeTimingSessionManager.Current.Measure("Initialize JSON:API (startup)"))
                {
                    app.UseJsonApi();
                }

                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }

            if (CodeTimingSessionManager.IsEnabled)
            {
                string timingResults = CodeTimingSessionManager.Current.GetResults();
                logger.LogInformation($"Measurement results for application startup:{Environment.NewLine}{timingResults}");
            }

            _codeTimingSession.Dispose();
        }
    }
}
