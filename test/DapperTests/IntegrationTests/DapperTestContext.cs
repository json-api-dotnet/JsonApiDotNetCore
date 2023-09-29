using System.Text.Json;
using DapperExample;
using DapperExample.Data;
using DapperExample.Models;
using DapperExample.Repositories;
using DapperExample.TranslationToSql.DataModel;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit.Abstractions;
using IClock = DapperExample.IClock;

namespace DapperTests.IntegrationTests;

[PublicAPI]
public sealed class DapperTestContext : IntegrationTest
{
    private const string SqlServerClearAllTablesScript = @"
        EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
        EXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?';
        EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL';";

    public static readonly DateTimeOffset FrozenTime = 29.September(2018).At(16, 41, 56).AsUtc().ToDateTimeOffset();

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
        return new WebApplicationFactory<TodoItem>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DapperExamplePostgreSql",
                $"Host=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=postgres;Password=postgres;Include Error Detail=true");

            builder.UseSetting("ConnectionStrings:DapperExampleMySql",
                $"Host=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=root;Password=mysql;SSL Mode=None");

            builder.UseSetting("ConnectionStrings:DapperExampleSqlServer",
                $"Server=localhost;Database=DapperExample-{Guid.NewGuid():N};User ID=sa;Password=Passw0rd!;TrustServerCertificate=true");

            builder.UseSetting("Logging:LogLevel:DapperExample", "Debug");

            builder.ConfigureLogging(loggingBuilder =>
            {
                if (_testOutputHelper != null)
                {
                    loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ => new XUnitLoggerProvider(_testOutputHelper, "DapperExample."));
                }
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IClock>(new FrozenClock
                {
                    UtcNow = FrozenTime
                });

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
