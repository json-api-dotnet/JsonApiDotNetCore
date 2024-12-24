using System.Text.Json;
using DapperExample;
using DapperExample.Data;
using DapperExample.Models;
using DapperExample.Repositories;
using DapperExample.TranslationToSql.DataModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit.Abstractions;

namespace DapperTests.IntegrationTests;

[PublicAPI]
public sealed class DapperTestContext : IntegrationTest
{
    private const string SqlServerClearAllTablesScript = """
        EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
        EXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?';
        EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
        """;

    public static readonly DateTimeOffset FrozenTime = DefaultDateTimeUtc;

    private readonly Lazy<WebApplicationFactory<TodoItem>> _lazyFactory;
    private ITestOutputHelper? _testOutputHelper;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = Factory.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public WebApplicationFactory<TodoItem> Factory => _lazyFactory.Value;

    public DapperTestContext()
    {
        _lazyFactory = new Lazy<WebApplicationFactory<TodoItem>>(CreateFactory);
    }

    private WebApplicationFactory<TodoItem> CreateFactory()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        // Justification: The child factory returned by WithWebHostBuilder() is owned by the parent factory, which disposes it.
        return new WebApplicationFactory<TodoItem>().WithWebHostBuilder(builder =>
#pragma warning restore CA2000 // Dispose objects before losing scope
        {
            builder.UseSetting("ConnectionStrings:DapperExamplePostgreSql",
                $"Host=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=postgres;Password=postgres;Include Error Detail=true");

            builder.UseSetting("ConnectionStrings:DapperExampleMySql",
                $"Host=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=root;Password=mysql;SSL Mode=None;AllowPublicKeyRetrieval=True");

            builder.UseSetting("ConnectionStrings:DapperExampleSqlServer",
                $"Server=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=sa;Password=Passw0rd!;TrustServerCertificate=true");

            builder.UseSetting("Logging:LogLevel:DapperExample", "Debug");

            builder.ConfigureLogging(loggingBuilder =>
            {
                if (_testOutputHelper != null)
                {
#if !DEBUG
                    // Reduce logging output when running tests in ci-build.
                    loggingBuilder.ClearProviders();
#endif
                    loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ => new XUnitLoggerProvider(_testOutputHelper, "DapperExample."));
                }
            });

            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FrozenTimeProvider(FrozenTime)));

                ServiceDescriptor scopedCaptureStore = services.Single(descriptor => descriptor.ImplementationType == typeof(SqlCaptureStore));
                services.Remove(scopedCaptureStore);

                services.AddSingleton<SqlCaptureStore>();
            });
        });
    }

    public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public async Task ClearAllTablesAsync(DbContext dbContext)
    {
        var dataModelService = Factory.Services.GetRequiredService<IDataModelService>();
        DatabaseProvider databaseProvider = dataModelService.DatabaseProvider;

        if (databaseProvider == DatabaseProvider.SqlServer)
        {
            await dbContext.Database.ExecuteSqlRawAsync(SqlServerClearAllTablesScript);
        }
        else
        {
            foreach (IEntityType entityType in dbContext.Model.GetEntityTypes())
            {
                string? tableName = entityType.GetTableName();

                string escapedTableName = databaseProvider switch
                {
                    DatabaseProvider.PostgreSql => $"\"{tableName}\"",
                    DatabaseProvider.MySql => $"`{tableName}`",
                    _ => throw new NotSupportedException($"Unsupported database provider '{databaseProvider}'.")
                };

#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                // Justification: Table names cannot be parameterized.
                await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {escapedTableName}");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }
        }
    }

    public async Task RunOnDatabaseAsync(Func<AppDbContext, Task> asyncAction)
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await asyncAction(dbContext);
    }

    public string AdaptSql(string text, bool hasClientGeneratedId = false)
    {
        var dataModelService = Factory.Services.GetRequiredService<IDataModelService>();
        var adapter = new SqlTextAdapter(dataModelService.DatabaseProvider);
        return adapter.Adapt(text, hasClientGeneratedId);
    }

    protected override HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    public override async Task DisposeAsync()
    {
        try
        {
            if (_lazyFactory.IsValueCreated)
            {
                try
                {
                    await RunOnDatabaseAsync(async dbContext => await dbContext.Database.EnsureDeletedAsync());
                }
                finally
                {
                    await _lazyFactory.Value.DisposeAsync();
                }
            }
        }
        finally
        {
            await base.DisposeAsync();
        }
    }
}
