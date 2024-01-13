using System.Diagnostics;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

/// <summary>
/// Base class for a test context that creates a new database and server instance before running tests and cleans up afterwards. You can either use this
/// as a fixture on your tests class (init/cleanup runs once before/after all tests) or have your tests class inherit from it (init/cleanup runs once
/// before/after each test). See <see href="https://xunit.net/docs/shared-context" /> for details on shared context usage.
/// </summary>
/// <typeparam name="TStartup">
/// The server Startup class, which can be defined in the test project or API project.
/// </typeparam>
/// <typeparam name="TDbContext">
/// The Entity Framework Core database context, which can be defined in the test project or API project.
/// </typeparam>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class IntegrationTestContext<TStartup, TDbContext> : IntegrationTest
    where TStartup : class
    where TDbContext : TestableDbContext
{
    private readonly Lazy<WebApplicationFactory<TStartup>> _lazyFactory;
    private readonly TestControllerProvider _testControllerProvider = new();
    private Action<ILoggingBuilder>? _loggingConfiguration;
    private Action<IServiceCollection>? _configureServices;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = Factory.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public WebApplicationFactory<TStartup> Factory => _lazyFactory.Value;

    public IntegrationTestContext()
    {
        _lazyFactory = new Lazy<WebApplicationFactory<TStartup>>(CreateFactory);
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

    private WebApplicationFactory<TStartup> CreateFactory()
    {
        string dbConnectionString = $"Host=localhost;Database=JsonApiTest-{Guid.NewGuid():N};User ID=postgres;Password=postgres;Include Error Detail=true";

        var factory = new IntegrationTestWebApplicationFactory();

        factory.ConfigureLogging(_loggingConfiguration);

        factory.ConfigureServices(services =>
        {
            _configureServices?.Invoke(services);

            services.ReplaceControllers(_testControllerProvider);

            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(dbConnectionString, builder => builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                SetDbContextDebugOptions(options);
            });
        });

        // We have placed an appsettings.json in the TestBuildingBlocks project directory and set the content root to there. Note that
        // controllers are not discovered in the content root, but are registered manually using IntegrationTestContext.UseController.
        WebApplicationFactory<TStartup> factoryWithConfiguredContentRoot =
            factory.WithWebHostBuilder(builder => builder.UseSolutionRelativeContentRoot($"test/{nameof(TestBuildingBlocks)}"));

        using IServiceScope scope = factoryWithConfiguredContentRoot.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        dbContext.Database.EnsureCreated();

        return factoryWithConfiguredContentRoot;
    }

    [Conditional("DEBUG")]
    private static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
    }

    public void ConfigureLogging(Action<ILoggingBuilder> loggingConfiguration)
    {
        _loggingConfiguration = loggingConfiguration;
    }

    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _configureServices = configureServices;
    }

    public async Task RunOnDatabaseAsync(Func<TDbContext, Task> asyncAction)
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await asyncAction(dbContext);
    }

    public override async Task DisposeAsync()
    {
        try
        {
            if (_lazyFactory.IsValueCreated)
            {
                await RunOnDatabaseAsync(async dbContext => await dbContext.Database.EnsureDeletedAsync());
                await _lazyFactory.Value.DisposeAsync();
            }
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    private sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<TStartup>
    {
        private Action<ILoggingBuilder>? _loggingConfiguration;
        private Action<IServiceCollection>? _configureServices;

        public void ConfigureLogging(Action<ILoggingBuilder>? loggingConfiguration)
        {
            _loggingConfiguration = loggingConfiguration;
        }

        public void ConfigureServices(Action<IServiceCollection>? configureServices)
        {
            _configureServices = configureServices;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:wrap_before_first_method_call true

            return Host
                .CreateDefaultBuilder(null)
                .ConfigureAppConfiguration(builder =>
                {
                    // For tests asserting on log output, we discard the log levels from appsettings.json and environment variables.
                    // But using appsettings.json for all other tests makes it easy to quickly toggle when debugging tests.
                    if (_loggingConfiguration != null)
                    {
                        builder.Sources.Clear();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services => _configureServices?.Invoke(services));
                    webBuilder.UseStartup<TStartup>();
                })
                .ConfigureLogging(options => _loggingConfiguration?.Invoke(options));

            // @formatter:wrap_before_first_method_call restore
            // @formatter:wrap_chained_method_calls restore
        }
    }
}
