using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.OpenApi;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
#if NET6_0
using Microsoft.AspNetCore.Authentication;
#endif

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

    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline.
    ConfigurePipeline(app);

    if (CodeTimingSessionManager.IsEnabled)
    {
        string timingResults = CodeTimingSessionManager.Current.GetResults();
        app.Logger.LogInformation($"Measurement results for application startup:{Environment.NewLine}{timingResults}");
    }

    return app;
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    using IDisposable _ = CodeTimingSessionManager.Current.Measure("Configure services");

#if NET6_0
    builder.Services.TryAddSingleton<ISystemClock, SystemClock>();
#else
    builder.Services.TryAddSingleton(TimeProvider.System);
#endif

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("Default");
        options.UseNpgsql(connectionString);

        SetDbContextDebugOptions(options);
    });

    IMvcCoreBuilder mvcCoreBuilder = builder.Services.AddMvcCore();

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
        }, discovery => discovery.AddCurrentAssembly(), mvcBuilder: mvcCoreBuilder);
    }

    using (CodeTimingSessionManager.Current.Measure("AddOpenApi()"))
    {
        builder.Services.AddOpenApi(mvcCoreBuilder);
    }
}

[Conditional("DEBUG")]
static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
}

static void ConfigurePipeline(WebApplication app)
{
    using IDisposable _ = CodeTimingSessionManager.Current.Measure("Configure pipeline");

    app.UseRouting();

    using (CodeTimingSessionManager.Current.Measure("UseJsonApi()"))
    {
        app.UseJsonApi();
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapControllers();
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
