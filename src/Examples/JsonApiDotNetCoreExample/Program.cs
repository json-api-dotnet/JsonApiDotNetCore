using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.OpenApi.Swashbuckle;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

[assembly: ExcludeFromCodeCoverage]

WebApplication app = CreateWebApplication(args);

if (!IsGeneratingOpenApiDocumentAtBuildTime())
{
    await CreateDatabaseAsync(app.Services);
}

await app.RunAsync();

static WebApplication CreateWebApplication(string[] args)
{
    using ICodeTimerSession codeTimerSession = new DefaultCodeTimerSession();
    CodeTimingSessionManager.Capture(codeTimerSession);

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    ConfigureServices(builder);

    builder.Services.AddOptions<SwaggerGenOptions>().Configure(options => options.OperationFilter<DynamicDocumentationOperationFilter>());

    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline.
    ConfigurePipeline(app);

    if (CodeTimingSessionManager.IsEnabled && app.Logger.IsEnabled(LogLevel.Information))
    {
        string timingResults = CodeTimingSessionManager.Current.GetResults();
        AppLog.LogStartupTimings(app.Logger, Environment.NewLine, timingResults);
    }

    return app;
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    using IDisposable _ = CodeTimingSessionManager.Current.Measure("Configure services");

    builder.Services.TryAddSingleton(TimeProvider.System);

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("Default");
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

    using (CodeTimingSessionManager.Current.Measure("AddOpenApiForJsonApi()"))
    {
        builder.Services.AddOpenApiForJsonApi(options => options.DocumentFilter<SetOpenApiServerAtBuildTimeFilter>());
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
    app.UseReDoc();
    app.MapScalarApiReference(options => options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json");

    app.MapControllers();
}

static bool IsGeneratingOpenApiDocumentAtBuildTime()
{
    return Environment.GetCommandLineArgs().Any(argument => argument.Contains("GetDocument.Insider"));
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
