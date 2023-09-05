using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: ExcludeFromCodeCoverage]

WebApplication app = CreateWebApplication(args);

await CreateDatabaseAsync(app.Services);

app.Run();

static WebApplication CreateWebApplication(string[] args)
{
    using ICodeTimerSession codeTimerSession = new DefaultCodeTimerSession();
    CodeTimingSessionManager.Capture(codeTimerSession);

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    ConfigureServices(builder);

    WebApplication webApplication = builder.Build();

    // Configure the HTTP request pipeline.
    ConfigurePipeline(webApplication);

    if (CodeTimingSessionManager.IsEnabled)
    {
        string timingResults = CodeTimingSessionManager.Current.GetResults();
        webApplication.Logger.LogInformation($"Measurement results for application startup:{Environment.NewLine}{timingResults}");
    }

    return webApplication;
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    using IDisposable _ = CodeTimingSessionManager.Current.Measure("Configure services");

    builder.Services.TryAddSingleton<ISystemClock, SystemClock>();

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        string? connectionString = GetConnectionString(builder.Configuration);
        options.UseNpgsql(connectionString);

        SetDbContextDebugOptions(options);
    });

    using (CodeTimingSessionManager.Current.Measure("AddJsonApi()"))
    {
        builder.Services.AddJsonApi<AppDbContext>(options =>
        {
            options.Namespace = "api";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

#if DEBUG
            options.IncludeExceptionStackTraceInErrors = true;
            options.IncludeRequestBodyInErrors = true;
            options.SerializerOptions.WriteIndented = true;
#endif
        }, discovery => discovery.AddCurrentAssembly());
    }
}

static string? GetConnectionString(IConfiguration configuration)
{
    string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
    return configuration.GetConnectionString("Default")?.Replace("###", postgresPassword);
}

[Conditional("DEBUG")]
static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
}

static void ConfigurePipeline(WebApplication webApplication)
{
    using IDisposable _ = CodeTimingSessionManager.Current.Measure("Configure pipeline");

    webApplication.UseRouting();

    using (CodeTimingSessionManager.Current.Measure("UseJsonApi()"))
    {
        webApplication.UseJsonApi();
    }

    webApplication.MapControllers();
}

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await dbContext.Database.EnsureCreatedAsync())
    {
        await Seeder.CreateSampleDataAsync(dbContext);
    }
}
