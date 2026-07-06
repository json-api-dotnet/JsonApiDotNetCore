using System.Diagnostics;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TestBuildingBlocks;

/// <summary>
/// Base class for a test context that creates a new database and server instance before running tests and cleans up afterward. You can either use this
/// as a fixture on your tests class (init/cleanup runs once before/after all tests) or have your tests class inherit from it (init/cleanup runs once
/// before/after each test). See <see href="https://xunit.net/docs/shared-context" /> for details on shared context usage.
/// </summary>
/// <typeparam name="TStartup">
/// The Startup class that configures the server.
/// </typeparam>
/// <typeparam name="TDbContext">
/// The Entity Framework Core database context that defines the entity models.
/// </typeparam>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class IntegrationTestContext<TStartup, TDbContext> : IntegrationTest, IAsyncLifetime
    where TStartup : IStartup, new()
    where TDbContext : TestableDbContext
{
    private readonly Lazy<WebApplication> _lazyApp;
    private readonly TestControllerProvider _testControllerProvider = new();
    private Action<ILoggingBuilder>? _loggingConfiguration;
    private Action<IServiceCollection>? _configureServices;
    private Action<IServiceCollection>? _postConfigureServices;
    private bool _throttleAcquired;

    private WebApplication App => _lazyApp.Value;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = App.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public FactoryBridge Factory
    {
        get
        {
            field ??= new FactoryBridge(App);
            return field;
        }
    }

    public IntegrationTestContext()
    {
        _lazyApp = new Lazy<WebApplication>(BuildApp, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public void UseController<TController>()
        where TController : ControllerBase
    {
        _testControllerProvider.AddController(typeof(TController));
    }

    protected override HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    private WebApplication BuildApp()
    {
        var startup = new TStartup();

        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = startup.GetType().Assembly.GetName().Name
        });

        _configureServices?.Invoke(builder.Services);
        startup.ConfigureServices(builder.Services);
        _postConfigureServices?.Invoke(builder.Services);

        builder.Services.TryAddSingleton<TimeProvider>(new FrozenTimeProvider(DefaultDateTimeUtc));
        builder.Services.ReplaceControllers(_testControllerProvider);

        string dbConnectionString =
            $"Host=localhost;Database=JsonApiTest-{Guid.NewGuid():N};User ID=postgres;Password=postgres;Include Error Detail=true;Command Timeout=600";

        builder.Services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(dbConnectionString, static optionsBuilder => optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            SetDbContextDebugOptions(options);
        });

        if (_loggingConfiguration == null)
        {
            ClearLoggingProvidersInReleaseBuild(builder.Logging);
        }
        else
        {
            _loggingConfiguration.Invoke(builder.Logging);
        }

        builder.Host.UseDefaultServiceProvider(ConfigureServiceProvider);
        builder.WebHost.UseTestServer();

        WebApplication app = builder.Build();
        startup.Configure(app);

        RunOnDatabase(app, static dbContext => dbContext.Database.EnsureCreated());

        app.Start();

        return app;
    }

    [Conditional("DEBUG")]
    private static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.ConfigureWarnings(static builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
    }

    [Conditional("RELEASE")]
    private static void ClearLoggingProvidersInReleaseBuild(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
    }

    public void ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        if (_loggingConfiguration != null && _loggingConfiguration != configureLogging)
        {
            throw new InvalidOperationException($"Do not call {nameof(ConfigureLogging)} multiple times.");
        }

        _loggingConfiguration = configureLogging;
    }

    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        if (_configureServices != null && _configureServices != configureServices)
        {
            throw new InvalidOperationException($"Do not call {nameof(ConfigureServices)} multiple times.");
        }

        _configureServices = configureServices;
    }

    public void PostConfigureServices(Action<IServiceCollection> postConfigureServices)
    {
        if (_postConfigureServices != null && _postConfigureServices != postConfigureServices)
        {
            throw new InvalidOperationException($"Do not call {nameof(PostConfigureServices)} multiple times.");
        }

        _postConfigureServices = postConfigureServices;
    }

    private void RunOnDatabase(WebApplication app, Action<TDbContext> action)
    {
        AssertThrottleAcquired();

        using IServiceScope scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        action(dbContext);
    }

    public async Task RunOnDatabaseAsync(Func<TDbContext, Task> asyncAction)
    {
        AssertThrottleAcquired();

        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await asyncAction(dbContext);
    }

    private void AssertThrottleAcquired()
    {
        if (!_throttleAcquired)
        {
            throw new InvalidOperationException("Database throttle not acquired.");
        }
    }

    public async Task InitializeAsync()
    {
        await AcquireDatabaseThrottleAsync();
        _throttleAcquired = true;
    }

    public virtual async Task DisposeAsync()
    {
        try
        {
            if (_lazyApp.IsValueCreated)
            {
                try
                {
                    await RunOnDatabaseAsync(static async dbContext => await dbContext.Database.EnsureDeletedAsync());
                }
                finally
                {
                    await App.StopAsync();
                    await App.DisposeAsync();
                }
            }
        }
        finally
        {
            ReleaseDatabaseThrottle();
        }
    }
}
